using System.Linq;
using DevExpress.Xpo;

namespace StatisticApp
{
    public sealed class ClientXPO : XPObject
    {
        public ClientXPO(Session session) : base(session)
        {
        }

        [Association("Client-StatisticData")]
        public XPCollection<StatisticDataXpo> StatisticData => GetCollection<StatisticDataXpo>("StatisticData");

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            AddFields();
        }

        public void AddFields()
        {
            foreach (
                var field in
                    StatisticApp.Fields.DBInfoFields.Concat(StatisticApp.Fields.HaspInfoFields)
                        .Concat(StatisticApp.Fields.OsInfoFields).Concat(StatisticApp.Fields.OtherFields))
                if (GetField(field) == null)
                    StatisticData.Add(new StatisticDataXpo(Session) {Name = field});
        }

        public StatisticDataXpo GetField(string name) => StatisticData.FirstOrDefault(item => item.Name == name);
    }
}