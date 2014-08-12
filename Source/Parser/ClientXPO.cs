using System;
using DevExpress.Xpo;

namespace StatisticApp
{
    public sealed class ClientXPO : XPObject
    {
        private string activationStatus;
        private DateTime activationStatusLastTime;
        private string appVersion;
        private DateTime appVersionLastTime;
        private int? empsCount;
        private DateTime empsCountLastTime;
        private string hasp;
        private bool? licAC;
        private DateTime licACLastTime;
        private bool? licBadj;
        private DateTime licBadjLastTime;
        private int? licEmps;
        private DateTime licEmpsLastTime;
        private int? licOpers;
        private DateTime licOpersLastTime;
        private int? licOpersSDK;
        private DateTime licOpersSDKLastTime;
        private bool? licPhoto;
        private DateTime licPhotoLastTime;
        private bool? licTA;
        private DateTime licTALastTime;
        private int? operatorsCount;
        private DateTime operatorsCountLastTime;
        private int? pullDevicesCount;
        private DateTime pullDevicesCountLastTime;
        private string supportTo;
        private DateTime supportToLastTime;
        private int? zkDevicesCount;
        private DateTime zkDevicesCountLastTime;

        public ClientXPO( Session session ) : base( session )
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public int? PullDevicesCount
        {
            get { return pullDevicesCount; }
            set { SetPropertyValue( "PullDevicesCount", ref pullDevicesCount, value ); }
        }

        public DateTime PullDevicesCountLastTime
        {
            get { return pullDevicesCountLastTime; }
            set { SetPropertyValue( "PullDevicesCountLastTime", ref pullDevicesCountLastTime, value ); }
        }

        public DateTime ZKDevicesCountLastTime
        {
            get { return zkDevicesCountLastTime; }
            set { SetPropertyValue( "ZKDevicesCountLastTime", ref zkDevicesCountLastTime, value ); }
        }

        public DateTime EmpsCountLastTime
        {
            get { return empsCountLastTime; }
            set { SetPropertyValue( "EmpsCountLastTime", ref empsCountLastTime, value ); }
        }

        public string AppVersion
        {
            get { return appVersion; }
            set { SetPropertyValue( "AppVersion", ref appVersion, value ); }
        }

        public DateTime AppVersionLastTime
        {
            get { return appVersionLastTime; }
            set { SetPropertyValue( "AppVersionLastTime", ref appVersionLastTime, value ); }
        }

        public string SupportTo
        {
            get { return supportTo; }
            set { SetPropertyValue( "SupportTo", ref supportTo, value ); }
        }

        public DateTime SupportToLastTime
        {
            get { return supportToLastTime; }
            set { SetPropertyValue( "SupportToLastTime", ref supportToLastTime, value ); }
        }

        public string ActivationStatus
        {
            get { return activationStatus; }
            set { SetPropertyValue( "ActivationStatus", ref activationStatus, value ); }
        }

        public DateTime ActivationStatusLastTime
        {
            get { return activationStatusLastTime; }
            set { SetPropertyValue( "ActivationStatusLastTime", ref activationStatusLastTime, value ); }
        }

        public int? EmpsCount
        {
            get { return empsCount; }
            set { SetPropertyValue( "EmpsCount", ref empsCount, value ); }
        }

        public DateTime OperatorsCountLastTime
        {
            get { return operatorsCountLastTime; }
            set { SetPropertyValue( "OperatorsCountLastTime", ref operatorsCountLastTime, value ); }
        }

        public int? OperatorsCount
        {
            get { return operatorsCount; }
            set { SetPropertyValue( "OperatorsCount", ref operatorsCount, value ); }
        }

        public int? ZKDevicesCount
        {
            get { return zkDevicesCount; }
            set { SetPropertyValue( "ZKDevicesCount", ref zkDevicesCount, value ); }
        }

        public int? LicEmps
        {
            get { return licEmps; }
            set { SetPropertyValue( "LicEmps", ref licEmps, value ); }
        }

        public DateTime LicEmpsLastTime
        {
            get { return licEmpsLastTime; }
            set { SetPropertyValue( "LicEmpsLastTime", ref licEmpsLastTime, value ); }
        }

        public bool? LicTA
        {
            get { return licTA; }
            set { SetPropertyValue( "LicTA", ref licTA, value ); }
        }

        public DateTime LicTALastTime
        {
            get { return licTALastTime; }
            set { SetPropertyValue( "LicTALastTime", ref licTALastTime, value ); }
        }

        public DateTime LicACLastTime
        {
            get { return licACLastTime; }
            set { SetPropertyValue( "LicACLastTime", ref licACLastTime, value ); }
        }

        public bool? LicPhoto
        {
            get { return licPhoto; }
            set { SetPropertyValue( "LicPhoto", ref licPhoto, value ); }
        }

        public DateTime LicPhotoLastTime
        {
            get { return licPhotoLastTime; }
            set { SetPropertyValue( "LicPhotoLastTime", ref licPhotoLastTime, value ); }
        }

        public bool? LicBadj
        {
            get { return licBadj; }
            set { SetPropertyValue( "LicBadj", ref licBadj, value ); }
        }

        public DateTime LicBadjLastTime
        {
            get { return licBadjLastTime; }
            set { SetPropertyValue( "LicBadjLastTime", ref licBadjLastTime, value ); }
        }

        public int? LicOpers
        {
            get { return licOpers; }
            set { SetPropertyValue( "LicOpers", ref licOpers, value ); }
        }

        public DateTime LicOpersLastTime
        {
            get { return licOpersLastTime; }
            set { SetPropertyValue( "LicOpersLastTime", ref licOpersLastTime, value ); }
        }

        public int? LicOpersSDK
        {
            get { return licOpersSDK; }
            set { SetPropertyValue( "LicOpersSDK", ref licOpersSDK, value ); }
        }

        public DateTime LicOpersSDKLastTime
        {
            get { return licOpersSDKLastTime; }
            set { SetPropertyValue( "LicOpersSDKLastTime", ref licOpersSDKLastTime, value ); }
        }

        public bool? LicAC
        {
            get { return licAC; }
            set { SetPropertyValue( "LicAC", ref licAC, value ); }
        }

        public string HASP
        {
            get { return hasp; }
            set { SetPropertyValue( "HASP", ref hasp, value ); }
        }

        [Association( "Client-Computers" )]
        public XPCollection< ComputerXPO > Computers
        {
            get { return GetCollection< ComputerXPO >( "Computers" ); }
        }
    }
}