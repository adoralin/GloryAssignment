using System.Net.NetworkInformation;

namespace GloryAssignment.Model
{
    /// <summary>
    /// This class of related information of the ip configuration.
    /// </summary>
    internal class IPConfigurationInformation
    {
        /// <summary>
        /// Adapter name.
        /// </summary>
        public string AdapterName { get; set; }
        /// <summary>
        /// IP address
        /// </summary>
        public string IPAddress {  get; set; }
        /// <summary>
        /// Status : Disabled, Disconnected, connected.
        /// </summary>
        public Status Status {  get; set; }
        /// <summary>
        /// Network Interface Type
        /// </summary>
        public string InterfaceType { get; set; }
        /// <summary>
        /// MAC-Address.
        /// </summary>
        public string MACAddress { get; set; }
        /// <summary>
        /// DNS Suffix.
        /// </summary>
        public string DNSSuffix {  get; set; }

        /// <summary>
        /// Subnetmask
        /// </summary>
        public string Subnetmask {  get; set; }
    }
}
