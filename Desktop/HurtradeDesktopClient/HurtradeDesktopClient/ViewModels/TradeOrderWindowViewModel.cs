﻿using HurtradeDesktopClient.Views;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using Prism.Commands;
using Prism.Mvvm;
using SharedData.poco;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow.Utils;
using SharedData.Services;

namespace HurtradeDesktopClient.ViewModels
{
    public delegate void TradeExecutedEventHandler(TradeOrderWindowViewModel context);

    public class TradeOrderWindowViewModel : BindableBase
    {

        #region Commands
        public DelegateCommand ExecuteTrade { get; private set; }
        public DelegateCommand WindowLoaded { get; private set; }
        public DelegateCommand WindowClosing { get; private set; }
        #endregion


        #region Events
        public event TradeExecutedEventHandler OnTradeExecuted;
        #endregion

        #region Properties
        public ObservableCollection<Quote> Quotes = new ObservableCollection<Quote>();

        private string _TradingSymbol = string.Empty;
        public string TradingSymbol
        {
            get
            {
                return _TradingSymbol;
            }
            set
            {
                SetProperty(ref _TradingSymbol, value);
            }
        }

        private string _WindowTitle = string.Empty;
        public string WindowTitle
        {
            get
            {
                return _WindowTitle;
            }
            set
            {
                SetProperty(ref _WindowTitle, value);
            }
        }

        private string _TradeButtonText;
        public string TradeButtonText
        {
            get { return _TradeButtonText; }
            set { SetProperty(ref _TradeButtonText, value); }
        }

        private string _CurrentPrice;
        public string CurrentPrice
        {
            get { return _CurrentPrice; }
            set { SetProperty(ref _CurrentPrice, value); }
        }

        private string _LotSize;
        public string LotSize
        {
            get { return _LotSize; }
            set { SetProperty(ref _LotSize, value); }
        }

        public bool IsBuy { get; set; }
        public TradeOrderWindow View { get; set; }
        #endregion


        #region Private Members
        private Object lockQuotes = new Object();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion


        public TradeOrderWindowViewModel()
        {
            SetupCommands();
        }

        private void SetupCommands()
        {
            ExecuteTrade = new DelegateCommand(ExecuteTradeCommand);
            WindowLoaded = new DelegateCommand(ExecuteWindowLoaded);
            WindowClosing = new DelegateCommand(ExecuteWindowClosing);
        }

        private void ExecuteWindowClosing()
        {
            ClientService.GetInstance().OnUpdateReceived -= OnUpdateReceived; ;
        }

        private void ExecuteWindowLoaded()
        {
            ClientService.GetInstance().OnUpdateReceived += OnUpdateReceived;
            TradeButtonText = (IsBuy ? "Buy " : "Sell ");
            WindowTitle = TradeButtonText + TradingSymbol;
        }

        private void OnUpdateReceived(object sender, SharedData.poco.updates.ClientUpdateEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (e.ClientQuotes.ContainsKey(TradingSymbol))
                {
                    SharedData.poco.Quote quote = e.ClientQuotes[TradingSymbol];
                    if (IsBuy)
                    {
                        CurrentPrice = "" + quote.Ask;
                    }
                    else
                    {
                        CurrentPrice = "" + quote.Bid;
                    }
                }
            });
        }

        private void ExecuteTradeCommand()
        {
            if (string.IsNullOrEmpty(LotSize)) { return; }
            if(string.IsNullOrWhiteSpace(LotSize)) { return; }

            decimal lotSize = 0;
            if (decimal.TryParse(LotSize, out lotSize))
            {
                OnTradeExecuted?.Invoke(this);
            }
        }

    }
}
