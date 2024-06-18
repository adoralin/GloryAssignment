using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;
using GloryAssignment.Model;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using GloryAssignment.MVVM;

namespace GloryAssignment.ViewModel
{
    /// <summary>
    /// ViewModel of the WPF application.
    /// </summary>
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IPConfigurationInformation> ConfigurationInformations
        { get; set; }
      
        public NetworkInterface[] NetworkInterfaces { get; set; }
        /// <summary>
        /// RelayCommand to update the adapters and related information.
        /// </summary>
        public RelayCommand UpdateCommand => new RelayCommand(execute => RefreshConfigurationInformation(), canExecute => { return true; });

        const string SELECTALL = "Select all adapters";

        /// <summary>
        /// Constructor of the view model.
        /// </summary>
        public MainWindowViewModel()
        {
            ConfigurationInformations = new ObservableCollection<IPConfigurationInformation>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertychanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Backing field of property AdapterNames.
        /// </summary>
        private IList<string> adapterNames;

        /// <summary>
        /// AdapterNames which will be displayed in the combobox for the user to select.
        /// </summary>
        public IList<string> AdapterNames
        {
            get
            {
                adapterNames = GetAdapterNames();
                return adapterNames;
            }
            set
            {
                if (adapterNames != value)
                {
                    value = adapterNames;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs($"{nameof(AdapterNames)}"));
                    }
                }
            }
        }

        /// <summary>
        /// Backing field of property SelectedAdapter.
        /// </summary>
        private string selectedAdapter;
        /// <summary>
        /// This property indicates the name of the selected adapter.
        /// </summary>
        public string SelectedAdapter
        {
            get { return selectedAdapter; }
            set
            {
                if (selectedAdapter != value)
                {
                    selectedAdapter = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs($"{nameof(SelectedAdapter)}"));
                        GetNetWorkInterfaceInformation(selectedAdapter);
                    }
                }
            }
        }

        /// <summary>
        /// This method updates the displayed adapter names and also the displayed information of the selected adapter.
        /// </summary>
        public void RefreshConfigurationInformation()
        {
            AdapterNames = GetAdapterNames().ToList();
            GetNetWorkInterfaceInformation(SelectedAdapter);
        }

        /// <summary>
        /// Gets all adapters.
        /// </summary>
        /// <returns></returns>
        private List<string> GetAdapterNames()
        {
            //Here if only ethernet type is desired we could also set the filter to Where(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            NetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces()?.Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback).ToArray();

            List<string> names = new List<string>();

            foreach (NetworkInterface adapter in NetworkInterfaces)
            {
                names.Add(adapter.Name);
            }

            names.Add(SELECTALL);
            return names;
        }

        /// <summary>
        /// Gets related information of a selected adapter.
        /// </summary>
        /// <param name="selectedAdapter"></param>
        /// <exception cref="ApplicationException"></exception>
        private void GetNetWorkInterfaceInformation(string selectedAdapter)
        {
            ConfigurationInformations.Clear();

            try
            {
                if (selectedAdapter == SELECTALL)
                {
                    foreach (NetworkInterface adapter in NetworkInterfaces)
                    {
                        GetConfigurationInformation(adapter);                     
                    }
                }
                else
                {
                    NetworkInterface adapter = NetworkInterfaces.FirstOrDefault(n => n.Name.Equals(selectedAdapter));

                    if (adapter != null)
                    {                      
                        GetConfigurationInformation(adapter);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Something went wrong during retrieving information about ip configuration.", ex.InnerException);
            }
        }

        /// <summary>
        /// Gets configuration information for a given adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="configuration"></param>
        private void GetConfigurationInformation(NetworkInterface adapter)
        {
            var configuration = new IPConfigurationInformation();
            configuration.AdapterName = adapter.Name;
            configuration.Status = GetStatus(adapter);
            GetInterfaceType(adapter, configuration);
            GetIPAddressAndSubnetmask(adapter, configuration);
            GetMacAddress(adapter, configuration);
            GetDNSSuffix(adapter, configuration);
            ConfigurationInformations.Add(configuration);
        }

        /// <summary>
        /// Gets network interface type.
        /// </summary>
        /// <param name="adapter"></param>
        private void GetInterfaceType(NetworkInterface adapter, IPConfigurationInformation configuration)
        {
            configuration.InterfaceType = adapter.OperationalStatus == OperationalStatus.Up ? adapter.NetworkInterfaceType.ToString() : "N/A";
        }

        /// <summary>
        /// Gets IP address of the given network interface.
        /// </summary>
        /// <param name="adapter">The adapter whose IP address will be retrieved.</param>
        private void GetIPAddressAndSubnetmask(NetworkInterface adapter, IPConfigurationInformation configuration)
        {
            if (adapter.OperationalStatus == OperationalStatus.Up)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in properties.UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        var address = unicastIPAddressInformation.Address;
                        var subnetmask = unicastIPAddressInformation.IPv4Mask;

                        configuration.IPAddress = address.ToString();
                        configuration.Subnetmask = subnetmask.ToString();
                    }
                }
            }
            else
            {
                configuration.IPAddress = "N/A";
                configuration.Subnetmask = "N/A";
            }
        }

        /// <summary>
        /// Gets MAC address of a given network interface.
        /// </summary>
        /// <param name="adapter">The given adapter whose MAC address to be retrieved.</param>
        private void GetMacAddress(NetworkInterface adapter, IPConfigurationInformation configuration)
        {
            if (adapter.OperationalStatus == OperationalStatus.Up)
            {
                string hexMac = adapter.GetPhysicalAddress().ToString(),
                regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})",
                replace = "$1:$2:$3:$4:$5:$6";
                string macAddress = Regex.Replace(hexMac, regex, replace);
                configuration.MACAddress = macAddress;
            }
            else
            {
                configuration.MACAddress = "N/A";
            }
        }

        /// <summary>
        /// Gets status of a given adapter.
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        private Status GetStatus(NetworkInterface adapter)
        {
            switch (adapter.OperationalStatus)
            {
                case OperationalStatus.Up: return Status.Connected;
                case OperationalStatus.Down:
                    if (adapter.GetIPProperties().GetIPv4Properties().IsDhcpEnabled)
                        return Status.Disconnected;
                    else return Status.Disabled;
                default: return Status.Disconnected;
            }
        }

        /// <summary>
        /// Gets DNS Suffix
        /// </summary>
        /// <param name="adapter"></param>
        private void GetDNSSuffix(NetworkInterface adapter, IPConfigurationInformation configuration)
        {
            configuration.DNSSuffix = adapter.GetIPProperties().DnsSuffix;
        }
    }
}
