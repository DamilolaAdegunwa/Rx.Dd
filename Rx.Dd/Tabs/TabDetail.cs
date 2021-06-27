namespace Rx.Dd.Tabs
{
    public class TabDetail
    {
        public string TabName { get; }
        public int TabId { get; }
        public double TabCompleteness { get; }

        public TabDetail(string tabName, int tabId, double tabCompleteness)
        {
            TabName = tabName;
            TabId = tabId;
            TabCompleteness = tabCompleteness;
        }
    }
}