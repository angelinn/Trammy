using SkgtService;
using SkgtService.Exceptions;
using SkgtService.Models;
using SkgtService.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TramlineFive.Common.Services;
using TramlineFive.DataAccess.Domain;

namespace TramlineFive.Common.ViewModels
{
    public class VirtualTablesViewModel : BaseViewModel
    {
        public async Task CheckForUpdatesAsync()
        {
            Version = await VersionService.CheckForUpdates();
        }

        public async Task SearchByStopCode()
        {
            IsLoading = true;

            StopInfo = await new ArrivalsService().GetByStopCodeAsync(stopCode);
            Direction = stopInfo.Lines.FirstOrDefault(l => !String.IsNullOrEmpty(l.Direction))?.Direction;
            await HistoryDomain.AddAsync(stopCode, stopInfo.Name);

            IsLoading = false;
        }

        private string direction;
        public string Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                RaisePropertyChanged();
            }
        }

        private StopInfo stopInfo;
        public StopInfo StopInfo
        {
            get
            {
                return stopInfo;
            }
            set
            {
                stopInfo = value;
                RaisePropertyChanged();
            }
        }

        private NewVersion version;
        public NewVersion Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
                RaisePropertyChanged();
            }
        }

        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                RaisePropertyChanged();
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
                RaisePropertyChanged();
            }
        }

        private string stopCode;
        public string StopCode
        {
            get
            {
                return stopCode;
            }
            set
            {
                stopCode = value;
                RaisePropertyChanged();
            }
        }
    }
}
