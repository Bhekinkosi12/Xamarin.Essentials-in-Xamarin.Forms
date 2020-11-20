using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RealEstateApp.Models;
using RealEstateApp.ViewModels.Base;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace RealEstateApp.ViewModels
{
    public class PropertyDetailViewModel : ViewModelBase
    {
        public ICommand EditPropertyCommand => new Command(EditPropertyAsync);
        public ICommand ViewPhotosCommand => 
            new Command(async () => await NavigationService.NavigateToModalAsync<ImageListViewModel>(Property));
        public ICommand ViewPanoramaCommand => 
            new Command(async () => await NavigationService.NavigateToModalAsync<PanoramaViewModel>(Property));

        public ICommand CancelSpeachCommand => new Command(CancelSpeachAsync);
        public ICommand SpeachCommand => new Command(SpeachDescriptorAsync);


        private CancellationTokenSource _cancelSpeachCancelationToken;
        public ObservableCollection<Locale> LocalesCollection { get; set; } = new ObservableCollection<Locale>();

        private async void CancelSpeachAsync()
        {
            if (_cancelSpeachCancelationToken?.IsCancellationRequested  ?? true)
            {
                return;
            }

            _cancelSpeachCancelationToken.Cancel();
            
        }

        private async void SpeachDescriptorAsync()
        {
            _cancelSpeachCancelationToken = new CancellationTokenSource();
            IsSpeaking = true;
            var options = new SpeechOptions()
            {
                Locale = this.SelectLocale,
                Volume = this.SelectVolume,
                Pitch = this.SelectPitch
            };

            await TextToSpeech.SpeakAsync(Property.Description,options, _cancelSpeachCancelationToken.Token);
            IsSpeaking = false;
        }

        public override void OnAppearing()
        {
        }

        public override void OnDisappearing()
        {
        }

        public override async Task InitializeAsync(object parameter)
        {
            Property = (Property)parameter;

            Agent = Repository.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);

            Repository.ObservePropertySaved()
                .Where(x => x.Id == Property.Id)
                .Subscribe(x => Property = x);

          
            var locales = await TextToSpeech.GetLocalesAsync();
            LocalesCollection = new ObservableCollection<Locale>(locales);
            OnPropertyChanged(nameof(LocalesCollection));

        }

        private Locale _selectLocale;

        public Locale SelectLocale
        {
            get { return _selectLocale; }
            set => SetProperty(ref _selectLocale, value);
        }

        private float _selectVolume = 1;

        public float SelectVolume
        {
            get { return _selectVolume; }
            set => SetProperty(ref _selectVolume, value);
        }

        private float _selectPitch =1;

        public float SelectPitch
        {
            get { return _selectPitch; }
            set => SetProperty(ref _selectPitch, value);
        }


        private bool _isSpecking;

        public bool IsSpeaking
        {
            get { return _isSpecking; }
            set => SetProperty(ref _isSpecking, value);
        }





        private Agent _agent;

        public Agent Agent
        {
            get => _agent;
            set => SetProperty(ref _agent, value);
        }

        private Property _property;

        public Property Property
        {
            get => _property;
            set => SetProperty(ref _property, value, forcePropertyChangedNotification: true);
        }

        public ICommand ToggleAdvancedSpeechCommand =>
            new Command(() => ShowAdvancedSpeechControls = !ShowAdvancedSpeechControls);

        private bool _showAdvancedSpeechControls;

        public bool ShowAdvancedSpeechControls
        {
            get => _showAdvancedSpeechControls;
            set => SetProperty(ref _showAdvancedSpeechControls, value);
        }

        private async void EditPropertyAsync()
        {
            await NavigationService.NavigateToModalAsync<AddEditPropertyViewModel>(Property);
        }
    }
}