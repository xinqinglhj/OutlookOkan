﻿using OutlookOkan.CsvTools;
using OutlookOkan.Properties;
using OutlookOkan.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutlookOkan.Models
{
    public class GenerateCheckList
    {
        private readonly CheckList _checkList = new CheckList();

        private readonly Dictionary<string, string> _displayNameAndRecipient = new Dictionary<string, string>();
        private readonly List<Whitelist> _whitelist = new List<Whitelist>();

        /// <summary>
        /// メール送信の確認画面を表示。
        /// </summary>
        /// <param name="mail">送信するメールアイテム</param>
        public CheckList GenerateCheckListFromMail(Outlook._MailItem mail)
        {
            MakeDisplayNameAndRecipient(mail);
            
            GetGeneralMailInfomation(mail);

            CheckForgotAttach(mail);

            CheckKeyword(mail);

            AutoAddCcAndBcc(mail);

            //TODO Temporary processing. It will be improved.
            MakeDisplayNameAndRecipient(mail);

            GetRecipient(mail);

            GetAttachmentsInfomation(mail);

            CheckMailbodyAndRecipient(mail);

            return _checkList;
        }

        private void GetGeneralMailInfomation(Outlook._MailItem mail)
        {
            try
            {
                _checkList.Sender = mail.SendUsingAccount.SmtpAddress;
            }
            catch (NullReferenceException)
            {
                _checkList.Sender = mail.SenderEmailAddress;

                if (!_checkList.Sender.Contains("@"))
                {
                    _checkList.Sender = Resources.FailedToGetInformation;
                }
            }
            _checkList.Subject = mail.Subject;
            _checkList.MailType = GetMailBodyFormat(mail);
            _checkList.MailBody = mail.Body;
            _checkList.MailHtmlBody = mail.HTMLBody;
        }

        /// <summary>
        /// 送信先の表示名と表示名とメールアドレスを対応させる。(Outlookの仕様上、表示名にメールアドレスが含まれない事がある。)
        /// </summary>
        /// <param name="mail"></param>
        private void MakeDisplayNameAndRecipient(Outlook._MailItem mail)
        {
            foreach (Outlook.Recipient recip in mail.Recipients)
            {
                // Exchangeの連絡先に登録された情報を取得。
                var exchangeUser = recip.AddressEntry.GetExchangeUser();

                // Exchangeの配布リスト(ML)として登録された情報を取得。
                var exchangeDistributionList = recip.AddressEntry.GetExchangeDistributionList();

                // ローカルの連絡先に登録された情報を取得。
                var registeredUser = recip.AddressEntry.GetContact();

                // 登録されたメールアドレスの場合、登録名のみが表示されるため、メールアドレスと共に表示されるよう表示用テキストを生成。
                var nameAndMailAddress = exchangeUser != null
                    ? exchangeUser.Name + $@" ({exchangeUser.PrimarySmtpAddress})"
                    : exchangeDistributionList != null
                        ? exchangeDistributionList.Name + $@" ({exchangeDistributionList.PrimarySmtpAddress})"
                        : registeredUser != null
                            ? recip.Name + $@" ({recip.Address})"
                            : recip.Address;

                _displayNameAndRecipient[recip.Name] = nameAndMailAddress;
            }
        }

        /// <summary>
        /// ファイルの添付忘れを確認。
        /// </summary>
        /// <param name="mail"></param>
        private void CheckForgotAttach(Outlook._MailItem mail)
        {
            if (mail.Body.Contains("添付") && mail.Attachments.Count == 0)
            {
                _checkList.Alerts.Add(new Alert{AlertMessage = Resources.ForgottenToAttachAlert,IsImportant = true, IsWhite = false, IsChecked = false});
            }
        }

        /// <summary>
        /// メールの形式を取得し、表示用の文字列を返す。
        /// </summary>
        /// <param name="mail"></param>
        /// <returns>メールの形式</returns>
        private string GetMailBodyFormat(Outlook._MailItem mail)
        {
            switch (mail.BodyFormat)
            {
                case Outlook.OlBodyFormat.olFormatUnspecified:
                    return Resources.Unknown;
                case Outlook.OlBodyFormat.olFormatPlain:
                    return Resources.Text;
                case Outlook.OlBodyFormat.olFormatHTML:
                    return Resources.HTML;
                case Outlook.OlBodyFormat.olFormatRichText:
                    return Resources.RichText;
                default:
                    return Resources.Unknown;
            }
        }

        /// <summary>
        /// 本文に登録したキーワードがある場合、登録した警告文を表示する。
        /// </summary>
        /// <param name="mail"></param>
        private void CheckKeyword(Outlook._MailItem mail)
        {
            //Load AlertKeywordAndMessage
            var csv = new ReadAndWriteCsv("AlertKeywordAndMessageList.csv");
            var alertKeywordAndMessageList = csv.GetCsvRecords<AlertKeywordAndMessage>(csv.LoadCsv<AlertKeywordAndMessageMap>());

            if (alertKeywordAndMessageList.Count != 0)
            {
                foreach (var i in alertKeywordAndMessageList)
                {
                    if (mail.Body.Contains(i.AlertKeyword))
                    {
                        _checkList.Alerts.Add(new Alert{AlertMessage = i.Message,IsImportant = true, IsWhite = false, IsChecked = false});
                        if (i.IsCanNotSend)
                        {
                            _checkList.IsCanNotSendMail = true;
                            _checkList.CanNotSendMailMessage = i.Message;
                        }
                    }
                }
            }
        }

        private void AutoAddCcAndBcc(Outlook._MailItem mail)
        {
            var autoAddedCcAddressList = new List<string>();
            var autoAddedBccAddressList = new List<string>();

            //Load AutoCcBccKeywordList
            var autoCcBccKeywordListCsv = new ReadAndWriteCsv("AutoCcBccKeywordList.csv");
            var autoCcBccKeywordList = autoCcBccKeywordListCsv.GetCsvRecords<AutoCcBccKeyword>(autoCcBccKeywordListCsv.LoadCsv<AutoCcBccKeywordMap>());

            if (autoCcBccKeywordList.Count != 0)
            {
                foreach (var i in autoCcBccKeywordList)
                {
                    if (mail.Body.Contains(i.Keyword) && !_displayNameAndRecipient.Any(recip => recip.Key.Contains(i.AutoAddAddress)))
                    {
                        var recip = mail.Recipients.Add(i.AutoAddAddress);
                        recip.Type = i.CcOrBcc == CcOrBcc.CC
                            ? (int) Outlook.OlMailRecipientType.olCC
                            : (int) Outlook.OlMailRecipientType.olBCC;

                        _checkList.Alerts.Add(new Alert{AlertMessage = Resources.AutoAddDestination + $@"[{i.CcOrBcc}] [{i.AutoAddAddress}] (" + Resources.ApplicableDestination + $" 「{i.Keyword}」)", IsImportant = false, IsWhite = true, IsChecked = true});

                        if (i.CcOrBcc == CcOrBcc.CC)
                        {
                            autoAddedCcAddressList.Add(i.AutoAddAddress);
                        }
                        else
                        {
                            autoAddedBccAddressList.Add(i.AutoAddAddress);
                        }

                        // 自動追加されたアドレスはホワイトリスト登録アドレス扱い。
                        _whitelist.Add(new Whitelist {WhiteName = i.AutoAddAddress});
                    }
                }
            }

            //Load AutoCcBccRecipientList
            // TODO To be improved
            var csv = new ReadAndWriteCsv("AutoCcBccRecipientList.csv");
            var autoCcBccRecipientList = csv.GetCsvRecords<AutoCcBccRecipient > (csv.LoadCsv<AutoCcBccRecipientMap>());

            if (autoCcBccRecipientList.Count != 0)
            {
                foreach (var i in autoCcBccRecipientList)
                {
                    if (_displayNameAndRecipient.Any(recipient => recipient.Value.Contains(i.TargetRecipient)) && !_displayNameAndRecipient.Any(recip => recip.Key.Contains(i.AutoAddAddress)))
                    {
                        if (i.CcOrBcc == CcOrBcc.CC)
                        {
                            if (!autoAddedCcAddressList.Contains(i.AutoAddAddress))
                            {
                                var recip = mail.Recipients.Add(i.AutoAddAddress);
                                recip.Type = (int)Outlook.OlMailRecipientType.olCC;

                                autoAddedCcAddressList.Add(i.AutoAddAddress);
                            }
                        }else if (!autoAddedBccAddressList.Contains(i.AutoAddAddress))
                            {
                                var recip = mail.Recipients.Add(i.AutoAddAddress);
                                recip.Type = (int)Outlook.OlMailRecipientType.olBCC;

                                autoAddedBccAddressList.Add(i.AutoAddAddress);
                            }

                        _checkList.Alerts.Add(new Alert{AlertMessage = Resources.AutoAddDestination + $@"[{i.CcOrBcc}] [{i.AutoAddAddress}] (" + Resources.ApplicableKeywords + $" 「{i.TargetRecipient}」)", IsImportant = false, IsWhite = true, IsChecked = true});

                        // 自動追加されたアドレスはホワイトリスト登録アドレス扱い。
                        _whitelist.Add(new Whitelist { WhiteName = i.AutoAddAddress });
                    }
                }
            }

            mail.Recipients.ResolveAll();
        }


        /// <summary>
        /// 添付ファイルとそのファイルサイズを取得し、チェックリストに追加する。
        /// </summary>
        /// <param name="mail"></param>
        private void GetAttachmentsInfomation(Outlook._MailItem mail)
        {
            if (mail.Attachments.Count != 0)
            {
                for (var i = 0; i < mail.Attachments.Count; i++)
                {
                    var fileSize = Math.Round(((double)mail.Attachments[i + 1].Size / 1024), 0, MidpointRounding.AwayFromZero).ToString("##,###") + "KB";

                    //10Mbyte以上の添付ファイルは警告も表示。
                    if (mail.Attachments[i + 1].Size >= 10485760)
                    {
                        _checkList.Alerts.Add(new Alert{AlertMessage = Resources.IsBigAttachedFile + $"[{mail.Attachments[i + 1].FileName}]", IsChecked = false, IsImportant = true, IsWhite = false});
                    }

                    var fileType = mail.Attachments[i + 1].FileName
                        .Substring(mail.Attachments[i + 1].FileName.LastIndexOf(".", StringComparison.Ordinal));

                    var isDangerous = false;
                    //実行ファイル(.exe)を添付していたら警告を表示
                    if (fileType == ".exe")
                    {
                        _checkList.Alerts.Add(new Alert{AlertMessage = Resources.IsAttachedExe + $"[{mail.Attachments[i + 1].FileName}]", IsChecked = false, IsImportant = true, IsWhite = false});
                        isDangerous = true;
                    }

                    _checkList.Attachments.Add(new Attachment{FileName = mail.Attachments[i + 1].FileName, FileSize = fileSize, FileType = fileType, IsTooBig  = mail.Attachments[i + 1].Size >= 10485760, IsEncrypted = false, IsChecked = false, IsDangerous = isDangerous});
                }
            }
        }

        /// <summary>
        /// 登録された名称とドメインから、宛先候補ではないアドレスが宛先に含まれている場合に、警告を表示する。
        /// </summary>
        /// <param name="mail"></param>
        private void CheckMailbodyAndRecipient(Outlook._MailItem mail)
        {
            //Load NameAndDomainsList
            var csv = new ReadAndWriteCsv("NameAndDomains.csv");
            var nameAndDomainsList = csv.GetCsvRecords<NameAndDomains>(csv.LoadCsv<NameAndDomainsMap>());

            //メールの本文中に、登録された名称があるか確認。
            var recipientCandidateDomains = (from nameAnddomain in nameAndDomainsList where mail.Body.Contains(nameAnddomain.Name) select nameAnddomain.Domain).ToList();

            //登録された名称かつ本文中に登場した名称以外のドメインが宛先に含まれている場合、警告を表示。
            //送信先の候補が見つからない場合、何もしない。(見つからない場合の方が多いため、警告ばかりになってしまう。) 
            if (recipientCandidateDomains.Count != 0)
            {
                foreach (var recipients in _displayNameAndRecipient)
                {
                    if (!recipientCandidateDomains.Any(
                        domains => domains.Equals(
                            recipients.Value.Substring(recipients.Value.IndexOf("@", StringComparison.Ordinal)))))
                    {
                        //送信者ドメインは警告対象外。
                        if (!recipients.Value.Contains(
                            mail.SendUsingAccount.SmtpAddress.Substring(
                                mail.SendUsingAccount.SmtpAddress.IndexOf("@", StringComparison.Ordinal))))
                        {
                            _checkList.Alerts.Add(new Alert{AlertMessage = recipients.Key + " : " + Resources.IsAlertAddressMaybeIrrelevant, IsImportant = true, IsWhite = false, IsChecked = false});
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 送信先メールアドレスを取得し、チェックリストに追加する。
        /// </summary>
        /// <param name="mail">送信するメールに関する情報</param>
        private void GetRecipient(Outlook._MailItem mail)
        {
            // TODO To be improved

            //Load Whitelist
            var readCsv = new ReadAndWriteCsv("Whitelist.csv");
            _whitelist.AddRange(readCsv.GetCsvRecords<Whitelist>(readCsv.LoadCsv<WhitelistMap>()));

            //Load AlertAddressList
            readCsv = new ReadAndWriteCsv("AlertAddressList.csv");
            var alertAddresslist = readCsv.GetCsvRecords<AlertAddress>(readCsv.LoadCsv<AlertAddressMap>());

            // 宛先(To,CC,BCC)に登録された宛先又は登録名を配列に格納。
            var toAdresses = mail.To?.Split(';') ?? Array.Empty<string>();
            var ccAdresses = mail.CC?.Split(';') ?? Array.Empty<string>();
            var bccAdresses = mail.BCC?.Split(';') ?? Array.Empty<string>();

            try
            {
                _checkList.SenderDomain =
                    mail.SendUsingAccount.SmtpAddress.Substring(
                        mail.SendUsingAccount.SmtpAddress.IndexOf("@", StringComparison.Ordinal));
            }
            catch (NullReferenceException)
            {
                try
                {
                    _checkList.SenderDomain =
                        mail.SenderEmailAddress.Substring(
                            mail.SenderEmailAddress.IndexOf("@", StringComparison.Ordinal));

                }
                catch (Exception)
                {
                    _checkList.SenderDomain = "--------------------";
                }
            }

            // 宛先や登録名から、表示用テキスト(メールアドレスや登録名)を各エリアに表示。
            // 宛先ドメインが送信元ドメインと異なる場合、色を変更するフラグをtrue、そうでない場合falseとする。
            // ホワイトリストに含まれる宛先の場合、ListのIsCheckedフラグをtrueにして、最初からチェック済みとする。
            // 警告アドレスリストに含まれる宛先の場合、AlertBoxにその旨を追加する。
            foreach (var i in _displayNameAndRecipient)
            {
                if (toAdresses.Any(address => address.Contains(i.Key)))
                {
                    var isExternal = !i.Value.Contains(_checkList.SenderDomain);
                    var isWhite = _whitelist.Count != 0 && _whitelist.Any(x => i.Value.Contains(x.WhiteName));
                    
                    _checkList.ToAddresses.Add(new Address{MailAddress = i.Value, IsExternal = isExternal, IsWhite = isWhite, IsChecked = isWhite});

                    if (alertAddresslist.Count != 0 && alertAddresslist.Any(address => i.Value.Contains(address.TartgetAddress)))
                    {
                        _checkList.Alerts.Add(new Alert{AlertMessage = Resources.IsAlertAddressToAlert + $"[{i.Value}]", IsImportant = true, IsWhite = false, IsChecked = false});

                        //送信禁止アドレスに該当する場合、禁止フラグを立て対象メールアドレスを説明文へ追加。
                        foreach (var alertAddress in alertAddresslist)
                        {
                            if(alertAddress.TartgetAddress == i.Value && alertAddress.IsCanNotSend)
                            {
                                _checkList.IsCanNotSendMail = true;
                                _checkList.CanNotSendMailMessage = Resources.SendingForbidAddress + $"[{i.Value}]";
                            }
                        }
                    }
                }

                if (ccAdresses.Any(address => address.Contains(i.Key)))
                {
                    var isExternal = !i.Value.Contains(_checkList.SenderDomain);
                    var isWhite = _whitelist.Count != 0 && _whitelist.Any(x => i.Value.Contains(x.WhiteName));

                    _checkList.CcAddresses.Add(new Address { MailAddress = i.Value, IsExternal = isExternal, IsWhite = isWhite, IsChecked = isWhite});

                    if (alertAddresslist.Count != 0 && alertAddresslist.Any(address => i.Value.Contains(address.TartgetAddress)))
                    {
                        _checkList.Alerts.Add(new Alert{AlertMessage = Resources.IsAlertAddressCcAlert + $"[{i.Value}]", IsImportant = true, IsWhite = false, IsChecked = false});

                        //送信禁止アドレスに該当する場合、禁止フラグを立て対象メールアドレスを説明文へ追加。
                        foreach (var alertAddress in alertAddresslist)
                        {
                            if (alertAddress.TartgetAddress == i.Value && alertAddress.IsCanNotSend)
                            {
                                _checkList.IsCanNotSendMail = true;
                                _checkList.CanNotSendMailMessage = Resources.SendingForbidAddress + $"[{i.Value}]";
                            }
                        }
                    }
                }

                if (bccAdresses.Any(address => address.Contains(i.Key)))
                {
                    var isExternal = !i.Value.Contains(_checkList.SenderDomain);
                    var isWhite = _whitelist.Count != 0 && _whitelist.Any(x => i.Value.Contains(x.WhiteName));

                    _checkList.BccAddresses.Add(new Address { MailAddress = i.Value, IsExternal = isExternal, IsWhite = isWhite, IsChecked = isWhite});

                    if (alertAddresslist.Count != 0 && alertAddresslist.Any(address => i.Value.Contains(address.TartgetAddress)))
                    {
                        _checkList.Alerts.Add(new Alert{AlertMessage = Resources.IsAlertAddressBccAlert + $"[{i.Value}]", IsImportant = true, IsWhite = false, IsChecked = false});
                        //送信禁止アドレスに該当する場合、禁止フラグを立て対象メールアドレスを説明文へ追加。
                        foreach (var alertAddress in alertAddresslist)
                        {
                            if (alertAddress.TartgetAddress == i.Value && alertAddress.IsCanNotSend)
                            {
                                _checkList.IsCanNotSendMail = true;
                                _checkList.CanNotSendMailMessage = Resources.SendingForbidAddress + $"[{i.Value}]";
                            }
                        }
                    }
                }
            }
        }
    }
}