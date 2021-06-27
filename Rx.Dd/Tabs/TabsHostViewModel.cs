using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Rx.Dd.Tests;

namespace Rx.Dd.Tabs
{
    public class TabsHostViewModel : ReactiveObject
    {
        private readonly SourceList<TabViewModel> _tabs = new SourceList<TabViewModel>();
        private TabDetail _selectedTab;
        private readonly SourceCache<Question, int> _questions = new SourceCache<Question, int>(x => x.Id);
        private ReadOnlyObservableCollection<Field> _currentQuestions;

        public TabsHostViewModel(IQuestionsService questionsService, IDocumentService documentService)
        {
            var tabChanges =
                _tabs
                    .Connect()
                    .RefCount();

            tabChanges
                .Sort(SortExpressionComparer<TabViewModel>.Ascending(t => t.TabId))
                .Bind(out var tabs)
                .Subscribe();

            tabChanges
                .AutoRefresh(x => x.TabCompleteness)
                .Transform(x => new TabDetail(x.TabName, x.TabId, x.TabCompleteness))
                .Bind(out var tabDetails)
                .Subscribe();

            var transformMany = tabChanges
                .TransformMany(x => x.Questions);

            transformMany
                .WhenPropertyChanged(x => x.AnswerText)
                .Throttle(TimeSpan.FromMilliseconds(300), RxApp.TaskpoolScheduler)
                .Subscribe(_ => questionsService.Save(_.Sender));

            Tabs = tabs;

            TabDetails = tabDetails;

            _tabs
                .AddRange(questionsService.GetQuestionCategories()
                    .Select(x => new TabViewModel(x.TabName, x.TabId, questionsService)));

            var currentQuestionsChanged =
                this.WhenAnyValue(x => x.SelectedTab)
                    .Where(x => x != null)
                    .SelectMany(x => documentService.GetQuestions(x.TabId))
                    .RefCount();

            currentQuestionsChanged
                .Bind(out var currentQuestions)
                .Subscribe();

            CurrentFields = currentQuestions;
            SelectedTab = tabDetails.First(x => x.TabId == 1); // QUESTION: [rlittlesii: June 26, 2021] Will this have a value in time?

            Initialize = ReactiveCommand.Create(() => { });

            Initialize
                .Subscribe(_ => SelectedTab = 1);
        }

        public ReactiveCommand<Unit, Unit> Initialize { get; set; }

        public TabDetail SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        public ReadOnlyObservableCollection<TabDetail> TabDetails { get; }
        public ReadOnlyObservableCollection<TabViewModel> Tabs { get; }
        public ReadOnlyObservableCollection<Field> CurrentFields { get; }
    }
}