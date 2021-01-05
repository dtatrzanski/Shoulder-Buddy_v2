﻿using DMapp.Services;
using DMapp.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DMapp.ViewModel
{
    class GeneralResultsVM
    {
        public Command EndCommand { get; set; }
        public Command DetailsCommand { get; set; }
        public Command BackCommand { get; set; }
        private int Mode; //mode 0 means, it will read data from temporary db. Mode 1 means it will read from sqlite db (we will use it when we open previously created decisionn session from the list)
        private int SessionID;
        INavigation navigation;
        public GeneralResultsVM(INavigation navi, int mode, int sessionID)
        {
            EndCommand = new Command(async () => await ExecuteEndCommand());
            BackCommand = new Command(async () => await ExecuteBackCommand());
            DetailsCommand = new Command(async () => await ExecuteDetailsCommand());
            Mode = mode;
            navigation = navi;
            SessionID = sessionID;
        }

        private async Task ExecuteBackCommand()
        {
           await navigation.PopAsync();
        }

        public void PageDisplayed()
        {
            if(Mode == 0)
            {
                //Obsolete
            }
            
        }

        private async Task ExecuteDetailsCommand()
        {
            if(Mode == 0)
            {
                TemporaryDb.PrepareDataBeforeInsertion();
            }
            
            await navigation.PushAsync(new DetailedResultsPage(SessionID, Mode));
        }

        private async Task ExecuteEndCommand()
        {
            if(Mode == 0)
            {
                TemporaryDb.PrepareDataBeforeInsertion();
                await Task.Run(() => TemporaryDb.InsertDataToSQLiteDB()); // make it async in the future to avoid blocking UI when huge number of data is inserted to sqlite data base.
            }
            await navigation.PopToRootAsync();
        }
    }
}
