using System.Collections.Generic;

namespace Rx.Dd.Tabs
{
    public interface IQuestionsService
    {
        IList<TabViewModel> GetQuestionCategories();

        IList<Question> GetQuestions(int id);

        void Save(Question question);
    }
}