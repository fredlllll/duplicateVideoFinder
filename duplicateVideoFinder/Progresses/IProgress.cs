namespace duplicateVideoFinder.Progresses
{
    public interface IProgress
    {
        float Progress
        {
            get;
        }

        string Status
        {
            get;
        }
    }
}
