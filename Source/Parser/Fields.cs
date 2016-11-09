using System.Collections.Generic;

namespace StatisticApp
{
    public static class Fields
    {
        public static readonly List<string> DBInfoFields = new List<string>
        {
            "AccessCategoryXPO",
            "BadgeLayoutXPO",
            "BaseCheckPointXPO",
            "CategoryBaseXPO",
            "CompanyXPO",
            "DailyRouteWithIndexXPO",
            "DailyRouteXPO",
            "DepartmentXPO",
            "DopSchituvatel1XPO",
            "EmployeesAccessCategoryXPO",
            "EmployerXPO",
            "ExternalSystemObjectXPO",
            "FilterCategoryXPO",
            "GrafikRabotiXPO",
            "GroupCategoryXPO",
            "GruppaKontrolnixTochekXPO",
            "HirschTochkaRegistraciiXPO",
            "KeriTochkaRegistraciiXPO",
            "KontrolnayaTochkaXPO",
            "KorrektirovkaXPO",
            "LogicalElementXPO",
            "LogicalLinkXPO",
            "MulticardCategoryXPO",
            "OblastXPO",
            "PhotoVerificationXPO",
            "PostXPO",
            "PullZKDevice1XPO",
            "PullZKTochkaRegistraciiXPO",
            "PunktDostupa2XPO",
            "RabochayaOblastXPO",
            "RabochayaStanciyaXPO",
            "ReportLayoutXPO",
            "ReportPluginXPO",
            "RoleXPO",
            "RoutePointXPO",
            "RoutesScheduleXPO",
            "RouteWithBeginTimeXPO",
            "RouteXPO",
            "RuleXPO",
            "SagemDevice1XPO",
            "SagemGruppaDostupaXPO",
            "SagemTochkaRegistraciiXPO",
            "SchedulesAccessCategoryXPO",
            "ShluzXPO",
            "SmartecDeviceXPO",
            "SmartecRegistrationPointXPO",
            "SmenaXPO",
            "SourceCheckPointXPO",
            "SystemPluginXPO",
            "TagCheckPointXPO",
            "TimexOperator2XPO",
            "TochkaRegistraciiXPO",
            "UrovenDostupaXPO",
            "UrovniDostupaAccessCategoryXPO",
            "VhodXPO",
            "VihodXPO",
            "VremennayaZonaXPO",
            "WorkingAreasAccessCategoryXPO",
            "ZKDevice1XPO",
            "ZKTochkaRegistraciiXPO"
        };

        public static readonly List<string> HaspInfoFields = new List<string>
        {
            "HaspID",
            "hostname",
            "ip",
            "osname",
            "ActivationStatus",
            "Количество операторов",
            "Количество операторов SDK",
            "Модуль рабочего времени",
            "Модуль контроля доступа",
            "Модуль фотоверификации",
            "Модуль печати",
            "Модуль дизайнера отчетов",
            "Модуль охранно-пожарной сигнализации",
            "Модуль видеонаблюдения",
            "Модуль guard tour",
            "Количество сотрудников для учета рабочего времени",
            "Поддержка до",
            "Дата компиляции"
        };

        public static readonly List<string> OsInfoFields = new List<string>
        {
            "OSVersion",
            "CommandLine",
            "MachineName",
            "UserName",
            "UserDomainName",
            "Is64BitOperatingSystem",
            "Is64BitProcess",
            "dotNetVersion",
            "ProcessorCount",
            "SystemDirectory",
            "LegacyV2RuntimeEnabledSuccessfully"
        };

        public static readonly List<string> OtherFields = new List<string>
        {
            "AppVersion"
        };
    }
}