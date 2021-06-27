using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

namespace Rx.Dd.Tabs
{
    public class TabViewModel : ReactiveObject
    {
        private ObservableAsPropertyHelper<double> _completedQuestions = ObservableAsPropertyHelper<double>.Default();
        private SourceList<Question> _questions = new SourceList<Question>();
        private ObservableAsPropertyHelper<double> _tabCompleteness;
        public int TabId { get; }
        public string TabName { get; }
        public double TabCompleteness => _tabCompleteness.Value;
        public ReadOnlyObservableCollection<Question> Questions { get; }
        public double CompletedQuestions => _completedQuestions.Value;

        public TabViewModel(string tabName, int id, IQuestionsService questionsService)
        {
            _questions.AddRange(questionsService.GetQuestions(id));
            TabId = id;
            TabName = tabName;
            var questionsConnection =
                _questions
                    .Connect()
                    .RefCount(); // RefCount will reuse a Connect(), rather than regenerating for each 'Subscribe'

            questionsConnection
                .AutoRefreshOnObservable(question => question.WhenAnyValue(x => x.IsCompleted))
                .ToCollection()
                .Select(collection => Enumerable.Count<Question>(collection, question => question.IsCompleted) / (double) collection.Count)
                .ToProperty(this, nameof(CompletedQuestions), out _tabCompleteness, () => 0);

            questionsConnection.Bind(out var questions).Subscribe();

            Questions = questions;
        }
    }
}