namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query
{
    public class QuestionSubmittedEvent
    {
        public QuestionSubmittedEvent(Question question)
        {
            Query = question;
        }
        public Question Query
        {
            get;
        }
    }
}
