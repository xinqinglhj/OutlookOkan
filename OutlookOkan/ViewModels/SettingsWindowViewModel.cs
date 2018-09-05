﻿using OutlookOkan.CsvTools;
using OutlookOkan.Services;
using OutlookOkan.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Input;

namespace OutlookOkan.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        public SettingsWindowViewModel()
        {
            //Add button command
            ImportWhiteList = new RelayCommand(ImportWhiteListFromCsv);
            ExportWhiteList = new RelayCommand(ExportWhiteListToCsv);

            ImportNameAndDomainsList = new RelayCommand(ImportNameAndDomainsFromCsv);
            ExportNameAndDomainsList = new RelayCommand(ExportNameAndDomainsToCsv);

            ImportAlertKeywordAndMessagesList = new RelayCommand(ImportAlertKeywordAndMessagesFromCsv);
            ExportAlertKeywordAndMessagesList = new RelayCommand(ExportAlertKeywordAndMessagesToCsv);

            ImportAlertAddressesList = new RelayCommand(ImportAlertAddressesFromCsv);
            ExportAlertAddressesList = new RelayCommand(ExportAlertAddressesToCsv);

            ImportAutoCcBccKeywordsList = new RelayCommand(ImportAutoCcBccKeywordsFromCsv);
            ExportAutoCcBccKeywordsList = new RelayCommand(ExportAutoCcBccKeywordsToCsv);

            ImportAutoCcBccRecipientsList = new RelayCommand(ImportAutoCcBccRecipientsFromCsv);
            ExportAutoCcBccRecipientsList = new RelayCommand(ExportAutoCcBccRecipientsToCsv);

            //言語コードと名称をロード
            var langlist = new Languages();
            Languages = langlist.Language;

            //Load settings from csv.
            LoadGeneralSettingData();
            LoadWhitelistData();
            LoadNameAndDomainsData();
            LoadAlertKeywordAndMessagesData();
            LoadAlertAddressessData();
            LoadAutoCcBccKeywordsData();
            LoadAutoCcBccRecipientsData();
        }

        public void SaveSettings()
        {
            SaveGeneralSettingToCsv();
            SaveWhiteListToCsv();
            SaveNameAndDomainsToCsv();
            SaveAlertKeywordAndMessageToCsv();
            SaveAlertAddressesToCsv();
            SaveAutoCcBccKeywordsToCsv();
            SaveAutoCcBccRecipientsToCsv();
        }

        #region Whitelist

        public ICommand ImportWhiteList { get; }
        public ICommand ExportWhiteList { get; }

        private void LoadWhitelistData()
        {
            var readCsv = new ReadAndWriteCsv("Whitelist.csv");
            foreach (var data in readCsv.GetCsvRecords<Whitelist>(readCsv.LoadCsv<WhitelistMap>()))
            {
                Whitelist.Add(data);
            }
        }

        private void SaveWhiteListToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in Whitelist) { tempDate.Add(data); }
            var writeCsv = new ReadAndWriteCsv("Whitelist.csv");
            writeCsv.WriteBindableDataToCsv<WhitelistMap>(tempDate);
        }

        private void ImportWhiteListFromCsv()
        {
            var importAction = new CsvImportAndExport();
            var filePath = importAction.ImportCsv();

            if (filePath != null)
            {
                try
                {
                    var importData =new List<Whitelist>(importAction.ReadCsv<Whitelist>(importAction.LoadCsv<WhitelistMap>(filePath)));
                    foreach (var data in importData)
                    {
                        Whitelist.Add(data);
                    }

                    MessageBox.Show(Properties.Resources.SuccessfulImport);
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.ImportFailed);
                }
            }
        }

        private void ExportWhiteListToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in Whitelist) { tempDate.Add(data); }

            var exportAction = new CsvImportAndExport();
            exportAction.CsvExport<WhitelistMap>(tempDate,"Whitelist.csv");
        }

        private ObservableCollection<Whitelist> _whitelist = new ObservableCollection<Whitelist>();
        public ObservableCollection<Whitelist> Whitelist
        {
            get => _whitelist;
            set
            {
                _whitelist = value;
                OnPropertyChanged("Whitelist");
            }
        }

        #endregion

        #region NameAndDomains

        public ICommand ImportNameAndDomainsList { get; }
        public ICommand ExportNameAndDomainsList { get; }

        private void LoadNameAndDomainsData()
        {
            var readCsv = new ReadAndWriteCsv("NameAndDomains.csv");
            foreach (var data in readCsv.GetCsvRecords<NameAndDomains>(readCsv.LoadCsv<NameAndDomainsMap>()))
            {
                NameAndDomains.Add(data);
            }
        }

        private void SaveNameAndDomainsToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in NameAndDomains) { tempDate.Add(data); }
            var writeCsv = new ReadAndWriteCsv("NameAndDomains.csv");
            writeCsv.WriteBindableDataToCsv<NameAndDomainsMap>(tempDate);
        }

        private void ImportNameAndDomainsFromCsv()
        {
            var importAction = new CsvImportAndExport();
            var filePath = importAction.ImportCsv();

            if (filePath != null)
            {
                try
                {
                    var importData = new List<NameAndDomains>(importAction.ReadCsv<NameAndDomains>(importAction.LoadCsv<NameAndDomainsMap>(filePath)));
                    foreach (var data in importData)
                    {
                        NameAndDomains.Add(data);
                    }

                    MessageBox.Show(Properties.Resources.SuccessfulImport);
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.ImportFailed);
                }
            }
        }

        private void ExportNameAndDomainsToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in NameAndDomains) { tempDate.Add(data); }

            var exportAction = new CsvImportAndExport();
            exportAction.CsvExport<NameAndDomainsMap>(tempDate, "NameAndDomains.csv");
        }

        private ObservableCollection<NameAndDomains> _nameAndDomains = new ObservableCollection<NameAndDomains>();
        public ObservableCollection<NameAndDomains> NameAndDomains
        {
            get => _nameAndDomains;
            set
            {
                _nameAndDomains = value;
                OnPropertyChanged("NameAndDomains");
            }
        }

        #endregion

        #region AlertKeywordAndMessages

        public ICommand ImportAlertKeywordAndMessagesList { get; }
        public ICommand ExportAlertKeywordAndMessagesList { get; }

        private void LoadAlertKeywordAndMessagesData()
        {
            var readCsv = new ReadAndWriteCsv("AlertKeywordAndMessageList.csv");
            foreach (var data in readCsv.GetCsvRecords<AlertKeywordAndMessage>(readCsv.LoadCsv<AlertKeywordAndMessageMap>()))
            {
                AlertKeywordAndMessages.Add(data);
            }
        }

        private void SaveAlertKeywordAndMessageToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AlertKeywordAndMessages) { tempDate.Add(data); }
            var writeCsv = new ReadAndWriteCsv("AlertKeywordAndMessageList.csv");
            writeCsv.WriteBindableDataToCsv<AlertKeywordAndMessageMap>(tempDate);
        }

        private void ImportAlertKeywordAndMessagesFromCsv()
        {
            var importAction = new CsvImportAndExport();
            var filePath = importAction.ImportCsv();

            if (filePath != null)
            {
                try
                {
                    var importData = new List<AlertKeywordAndMessage>(importAction.ReadCsv<AlertKeywordAndMessage>(importAction.LoadCsv<AlertKeywordAndMessageMap>(filePath)));
                    foreach (var data in importData)
                    {
                        AlertKeywordAndMessages.Add(data);
                    }

                    MessageBox.Show(Properties.Resources.SuccessfulImport);
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.ImportFailed);
                }
            }
        }

        private void ExportAlertKeywordAndMessagesToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AlertKeywordAndMessages) { tempDate.Add(data); }

            var exportAction = new CsvImportAndExport();
            exportAction.CsvExport<AlertKeywordAndMessageMap>(tempDate, "AlertKeywordAndMessageList.csv");
        }

        private ObservableCollection<AlertKeywordAndMessage> _alertKeywordAndMessages = new ObservableCollection<AlertKeywordAndMessage>();
        public ObservableCollection<AlertKeywordAndMessage> AlertKeywordAndMessages
        {
            get => _alertKeywordAndMessages;
            set
            {
                _alertKeywordAndMessages = value;
                OnPropertyChanged("AlertKeywordAndMessages");
            }
        }

        #endregion

        #region AlertAddresses

        public ICommand ImportAlertAddressesList { get; }
        public ICommand ExportAlertAddressesList { get; }

        private void LoadAlertAddressessData()
        {
            var readCsv = new ReadAndWriteCsv("AlertAddressList.csv");
            foreach (var data in readCsv.GetCsvRecords<AlertAddress>(readCsv.LoadCsv<AlertAddressMap>()))
            {
                AlertAddresses.Add(data);
            }
        }

        private void SaveAlertAddressesToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AlertAddresses) { tempDate.Add(data); }
            var writeCsv = new ReadAndWriteCsv("AlertAddressList.csv");
            writeCsv.WriteBindableDataToCsv<AlertAddressMap>(tempDate);
        }

        private void ImportAlertAddressesFromCsv()
        {
            var importAction = new CsvImportAndExport();
            var filePath = importAction.ImportCsv();

            if (filePath != null)
            {
                try
                {
                    var importData = new List<AlertAddress>(importAction.ReadCsv<AlertAddress>(importAction.LoadCsv<AlertAddressMap>(filePath)));
                    foreach (var data in importData)
                    {
                        AlertAddresses.Add(data);
                    }

                    MessageBox.Show(Properties.Resources.SuccessfulImport);
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.ImportFailed);
                }
            }
        }

        private void ExportAlertAddressesToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AlertAddresses) { tempDate.Add(data); }

            var exportAction = new CsvImportAndExport();
            exportAction.CsvExport<AlertAddressMap>(tempDate, "AlertAddressList.csv");
        }

        private ObservableCollection<AlertAddress> _alertAddresses = new ObservableCollection<AlertAddress>();
        public ObservableCollection<AlertAddress> AlertAddresses
        {
            get => _alertAddresses;
            set
            {
                _alertAddresses = value;
                OnPropertyChanged("AlertAddresses");
            }
        }

        #endregion

        #region  AutoCcBccKeywords

        public ICommand ImportAutoCcBccKeywordsList { get; }
        public ICommand ExportAutoCcBccKeywordsList { get; }

        private void LoadAutoCcBccKeywordsData()
        {
            var readCsv = new ReadAndWriteCsv("AutoCcBccKeywordList.csv");
            foreach (var data in readCsv.GetCsvRecords<AutoCcBccKeyword>(readCsv.LoadCsv<AutoCcBccKeywordMap>()))
            {
                AutoCcBccKeywords.Add(data);
            }
        }

        private void SaveAutoCcBccKeywordsToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AutoCcBccKeywords) { tempDate.Add(data); }
            var writeCsv = new ReadAndWriteCsv("AutoCcBccKeywordList.csv");
            writeCsv.WriteBindableDataToCsv<AutoCcBccKeywordMap>(tempDate);
        }

        private void ImportAutoCcBccKeywordsFromCsv()
        {
            var importAction = new CsvImportAndExport();
            var filePath = importAction.ImportCsv();

            if (filePath != null)
            {
                try
                {
                    var importData = new List<AutoCcBccKeyword>(importAction.ReadCsv<AutoCcBccKeyword>(importAction.LoadCsv<AutoCcBccKeywordMap>(filePath)));
                    foreach (var data in importData)
                    {
                        AutoCcBccKeywords.Add(data);
                    }

                    MessageBox.Show(Properties.Resources.SuccessfulImport);
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.ImportFailed);
                }
            }
        }

        private void ExportAutoCcBccKeywordsToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AutoCcBccKeywords) { tempDate.Add(data); }

            var exportAction = new CsvImportAndExport();
            exportAction.CsvExport<AutoCcBccKeywordMap>(tempDate, "AutoCcBccKeywordList.csv");
        }

        private ObservableCollection<AutoCcBccKeyword> _autoCcBccKeywords = new ObservableCollection<AutoCcBccKeyword>();
        public ObservableCollection<AutoCcBccKeyword> AutoCcBccKeywords
        {
            get => _autoCcBccKeywords;
            set
            {
                _autoCcBccKeywords = value;
                OnPropertyChanged("AutoCcBccKeywords");
            }
        }

        #endregion

        #region AutoCcBccRecipient

        public ICommand ImportAutoCcBccRecipientsList { get; }
        public ICommand ExportAutoCcBccRecipientsList { get; }

        private void LoadAutoCcBccRecipientsData()
        {
            var readCsv = new ReadAndWriteCsv("AutoCcBccRecipientList.csv");
            foreach (var data in readCsv.GetCsvRecords<AutoCcBccRecipient>(readCsv.LoadCsv<AutoCcBccRecipientMap>()))
            {
                AutoCcBccRecipients.Add(data);
            }
        }

        private void SaveAutoCcBccRecipientsToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AutoCcBccRecipients) { tempDate.Add(data); }
            var writeCsv = new ReadAndWriteCsv("AutoCcBccRecipientList.csv");
            writeCsv.WriteBindableDataToCsv<AutoCcBccRecipientMap>(tempDate);
        }

        private void ImportAutoCcBccRecipientsFromCsv()
        {
            var importAction = new CsvImportAndExport();
            var filePath = importAction.ImportCsv();

            if (filePath != null)
            {
                try
                {
                    var importData = new List<AutoCcBccRecipient>(importAction.ReadCsv<AutoCcBccRecipient>(importAction.LoadCsv<AutoCcBccRecipientMap>(filePath)));
                    foreach (var data in importData)
                    {
                        AutoCcBccRecipients.Add(data);
                    }

                    MessageBox.Show(Properties.Resources.SuccessfulImport);
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.ImportFailed);
                }
            }
        }

        private void ExportAutoCcBccRecipientsToCsv()
        {
            var tempDate = new BindingSource();
            foreach (var data in AutoCcBccRecipients) { tempDate.Add(data); }

            var exportAction = new CsvImportAndExport();
            exportAction.CsvExport<AutoCcBccRecipientMap>(tempDate, "AutoCcBccRecipientList.csv");
        }

        private ObservableCollection<AutoCcBccRecipient> _autoCcBccRecipients = new ObservableCollection<AutoCcBccRecipient>();
        public ObservableCollection<AutoCcBccRecipient> AutoCcBccRecipients
        {
            get => _autoCcBccRecipients;
            set
            {
                _autoCcBccRecipients = value;
                OnPropertyChanged("AutoCcBccRecipients");
            }
        }

        #endregion

        #region GeneralSetting

        private void LoadGeneralSettingData()
        {
            var readCsv = new ReadAndWriteCsv("GeneralSetting.csv");

            //1行しかないはずだが、何かの間違いで2行以上あるとまずいので、全行ロードする。
            foreach (var data in readCsv.GetCsvRecords<GeneralSetting>(readCsv.LoadCsv<GeneralSettingMap>()))
            {
                _generalSetting.Add((data));
            }

            //実際に使用するのは1行目の設定のみ
            //現在の言語はここではロードしない。
            if (_generalSetting.Count != 0)
            {
                IsDoNotConfirmationIfAllRecipientsAreSameDomain = _generalSetting[0].IsDoNotConfirmationIfAllRecipientsAreSameDomain;
                IsDoDoNotConfirmationIfAllWhite = _generalSetting[0].IsDoDoNotConfirmationIfAllWhite;
                IsAutoCheckIfAllRecipientsAreSameDomain = _generalSetting[0].IsAutoCheckIfAllRecipientsAreSameDomain;
            }
        }

        private void SaveGeneralSettingToCsv()
        {
            var languageCode = Language == null ? CultureInfo.CurrentUICulture.Name : Language.LanguageCode;
            if (Language != null)
            {
                ResourceService.Instance.ChangeCulture(Language.LanguageCode);
            }

            var tempGeneralSetting = new List<GeneralSetting>
            {
                new GeneralSetting
                {

                    IsDoNotConfirmationIfAllRecipientsAreSameDomain = IsDoNotConfirmationIfAllRecipientsAreSameDomain,
                    IsDoDoNotConfirmationIfAllWhite = IsDoDoNotConfirmationIfAllWhite,
                    IsAutoCheckIfAllRecipientsAreSameDomain = IsAutoCheckIfAllRecipientsAreSameDomain,
                    LanguageCode = languageCode
                }
            };

            var tempDate = new BindingSource();
            foreach (var data in tempGeneralSetting) { tempDate.Add(data); }
            var writeCsv = new ReadAndWriteCsv("GeneralSetting.csv");
            writeCsv.WriteBindableDataToCsv<GeneralSettingMap>(tempDate);
        }

        private readonly List<GeneralSetting> _generalSetting = new List<GeneralSetting>();

        private bool _isDoNotConfirmationIfAllRecipientsAreSameDomain;
        public bool IsDoNotConfirmationIfAllRecipientsAreSameDomain
        {
            get => _isDoNotConfirmationIfAllRecipientsAreSameDomain;
            set
            {
                _isDoNotConfirmationIfAllRecipientsAreSameDomain = value;
                OnPropertyChanged("IsDoNotConfirmationIfAllRecipientsAreSameDomain");
            }
        }

        private bool _isDoDoNotConfirmationIfAllWhite;
        public bool IsDoDoNotConfirmationIfAllWhite
        {
            get => _isDoDoNotConfirmationIfAllWhite;
            set
            {
                _isDoDoNotConfirmationIfAllWhite = value;
                OnPropertyChanged("IsDoDoNotConfirmationIfAllWhite");
            }
        }

        private bool _isAutoCheckIfAllRecipientsAreSameDomain;
        public bool IsAutoCheckIfAllRecipientsAreSameDomain
        {
            get => _isAutoCheckIfAllRecipientsAreSameDomain;
            set
            {
                _isAutoCheckIfAllRecipientsAreSameDomain = value;
                OnPropertyChanged("IsAutoCheckIfAllRecipientsAreSameDomain");
            }
        }

        private LanguageCodeAndName _language;
        public LanguageCodeAndName Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged("Language");
            }
        }

        private List<LanguageCodeAndName> _languages;
        public List<LanguageCodeAndName> Languages
        {
            get => _languages;
            set
            {
                _languages = value;
                OnPropertyChanged("Languages");
            }
        }

        #endregion
    }
}