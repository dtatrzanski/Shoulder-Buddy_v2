using DMapp.Models;
using DMapp.Services;
using DMapp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;

namespace DMapp.ViewModel
{
    class GeneralResultsVM: BaseViewModel
    {
        public Command EndCommand { get; set; }
        public Command DetailsCommand { get; set; }
        public Command BackCommand { get; set; }

        private ObservableCollection<WeightQualityChartModel> chartData { get; set; }  

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
            if(Mode == 1)
            {
                DecisionSession session = ManagerSQL.ReadDecisionSessions().Where(x => x.SessionID == SessionID).FirstOrDefault();
                int categoryID = session.SessionCategoryID;
                SessionCategory sessionCategory = ManagerSQL.ReadSessionCategories().Where(x => x.SessionCategoryID == categoryID).FirstOrDefault();
                string sessionCategoryName = sessionCategory.CategoryName;
                TemporaryDb.SessionCategoryName = sessionCategoryName;
            }
            else
            {
                TemporaryDb.PrepareDataBeforeInsertion();
            }
           
                UpdateGraph();
            
            
        }

        private void UpdateGraph()
        {
            List<string> qualityNames = new List<string>();
            List<double> weights = new List<double>();
            List<Weight> weightsClasses = new List<Weight>();
            List<Option> options = new List<Option>();
            List<double> qualitiesImportance = new List<double>();
           
            
            if(Mode == 0)
            {
                qualityNames = TemporaryDb.qualityNames;
                weights = TemporaryDb.weights;
                options = TemporaryDb.Options;
                qualitiesImportance = TemporaryDb.qualitiesImportance;
                weightsClasses = TemporaryDb.WeightClasses;
                
            }

            if(Mode == 1)
            {
                var allsession = ManagerSQL.ReadDecisionSessions();
                var allOptions = ManagerSQL.ReadOptions();
                qualityNames = ManagerSQL.ReadQualities().Where(x => x.SessionID == SessionID).Select(x => x.Name).ToList();
                options = ManagerSQL.ReadOptions().Where(x => x.SessionID == SessionID).ToList();
                qualitiesImportance = ManagerSQL.ReadQualities().Where(x => x.SessionID == SessionID).Select(x => x.Importance).ToList();
                weights = ManagerSQL.ReadWeights().Where(x => x.SessionID == SessionID).Select(x => x.Amount).ToList();
                weightsClasses = ManagerSQL.ReadWeights().Where(x => x.SessionID == SessionID).ToList();
            }




            
            List<List<double>> weightsToPass = new List<List<double>>();
            int cycleCounter = 1;
            int numOfQualities = qualitiesImportance.Count;
            foreach (var option in options)
            {
                List<double> weightsForOneOption = new List<double>();
                for (int i = (cycleCounter - 1) * numOfQualities; i < numOfQualities * cycleCounter; i++)
                {
                    weightsForOneOption.Add(weights[i]);

                }
                cycleCounter++;
                weightsToPass.Add(weightsForOneOption);


            }

            var optionsScore = DecisionSystem.ReturnResult(qualitiesImportance, weightsToPass).ToList();

            double temp = 0;
            int greatestScoreIndex = 0;
            for(int i = 0; i < optionsScore.Count; i++)
            {
                if(optionsScore[i] > temp) { temp = optionsScore[i]; greatestScoreIndex = i; }
            }
            Option bestOptionClass = options[greatestScoreIndex];
            BestOptionName = bestOptionClass.Name;
            List<Weight> bestOptionsWeights = new List<Weight>();
            if (Mode == 1)
            {
                bestOptionsWeights = weightsClasses.Where(x => x.OptionID == bestOptionClass.OptionID).ToList();
            }
            else
            {
                int starter = greatestScoreIndex * (qualityNames.Count);
                int finisher = (greatestScoreIndex + 1 ) * (qualityNames.Count);
                for (int i = starter; i < finisher; i++)
                {
                    bestOptionsWeights.Add(weightsClasses[i]);
                }

            }

            
             

            int counter = 0;

            ObservableCollection<WeightQualityChartModel> tempChartData = new ObservableCollection<WeightQualityChartModel>();

            foreach(var weight in bestOptionsWeights)
            {
                string qualityNameShorten = qualityNames[counter];
                int maxLenght = 6;
                if( maxLenght <= qualityNameShorten.Length) { qualityNameShorten = qualityNameShorten.Substring(0, 5) + "."; }
                else { qualityNameShorten = qualityNameShorten.Substring(0, qualityNameShorten.Length); }
                WeightQualityChartModel model = new WeightQualityChartModel
                {
                    QualityName = qualityNameShorten,
                    WeightAmount = bestOptionsWeights[counter].Amount * 100
                };
                tempChartData.Add(model);
                counter++;
            }

            ChartData = tempChartData;


           
           

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

        

        public ObservableCollection<WeightQualityChartModel> ChartData
        {
            get { return chartData; }
            set
            {
                chartData = value;
                OnPropertyChanged();
            }
        }

        private string bestOptionName;

        public string BestOptionName
        {
            get { return bestOptionName; }
            set { bestOptionName = value;
                OnPropertyChanged();
            }
        }



    }
}
