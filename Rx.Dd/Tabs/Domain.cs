using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using Rx.Dd.Tabs;

namespace Rx.Dd.Tests
{
    public class TaskDueDateConfigByPriority
    {
        public int TaskPriorityId { get; set; }
        public int TaskDueIn { get; set; }
        public string DatePart { get; set; }
    }

    public class AdditionalConfig
    {
        public bool DisplayUnqualifiedField { get; set; }
        public bool IsTaskDueDatesByPriorityCustomised { get; set; }
        public List<TaskDueDateConfigByPriority> TaskDueDateConfigByPriorities { get; set; }
    }

    public class Field : ReactiveObject
    {
        public Field()
        {
            this.WhenAnyValue(x => x.AnswerText,
                    x => x.IsMandatory,
                    (answer, mandatory) => (answer, mandatory))
                .Where(x => x.mandatory)
                .Where(x => !string.IsNullOrWhiteSpace(x.answer))
                .Select(x => x.answer)
                .Select(Validate)
                .BindTo(this, x => x.IsValid);
        }

        protected virtual bool Validate(string thing)
        {
            return true;
        }

        private string _answerText;
        private bool _isValid;
        public int Id { get; set; }
        public int TabId { get; set; }
        public int FieldTypeId { get; set; }
        public object MessageText { get; set; }

        public string AnswerText
        {
            get => _answerText;
            set => this.RaiseAndSetIfChanged(ref _answerText, value);
        }
        public int SortOrder { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsValid
        {
            get => _isValid;
            set => this.RaiseAndSetIfChanged(ref _isValid, value);
        }
    }

    public class FieldOfType : Field
    {
        protected override bool Validate(string thing)
        {
            return base.Validate(thing);
        }
    }

    public class Tab
    {
        public int SortOrder { get; set; }
        public List<Field> Fields { get; set; }
        public bool CanBeMarkedAsNotApplicable { get; set; }
        public int Id { get; set; }
        public string Caption { get; set; }
        public object Tooltip { get; set; }
        public int UiKeyId { get; set; }
        public object UiTooltipKeyId { get; set; }
        public object MessageText { get; set; }
        public object MessageUiKeyId { get; set; }
        public bool TabIsValid { get; set; }
        public bool TabNotApplicable { get; set; }
    }

    public class Root
    {
        public object ManagingAgentName { get; set; }
        public int EntityGroupId { get; set; }
        public bool ChecklistCanEditAfterDueDate { get; set; }
        public List<Tab> Tabs { get; set; }
        public int Id { get; set; }
        public string Caption { get; set; }
        public object Tooltip { get; set; }
        public int UiKeyId { get; set; }
        public object UiTooltipKeyId { get; set; }
        public object MessageText { get; set; }
        public object MessageUiKeyId { get; set; }
    }
    
    public interface IDocumentService
    {
        IObservable<IChangeSet<Field, int>> GetQuestions(int id);
        void Save(Field field);
    }

    public class DocumentService : IDocumentService
    {
        private readonly IDocumentApiClient _apiClient;
        private readonly ICacheService _cacheService;
        private readonly SourceCache<Field, int> _questions = new SourceCache<Field, int>(x => x.Id);
        private readonly IObservable<IChangeSet<Field, int>> _questionChanges;

        public DocumentService(IDocumentApiClient apiClient, ICacheService cacheService)
        {
            _apiClient = apiClient;
            _cacheService = cacheService;
            // todo: <Rodney Littles II: June 26, 2021> _questions needs to get data from the server and add to the SourceCache
            _questionChanges =
                _questions
                    .Connect()
                    .RefCount();

            _questionChanges
                .AutoRefresh(x => x.IsValid)
                .Filter(x => x.IsValid)
                .WhenPropertyChanged(x => x.AnswerText)
                .Subscribe(x => cacheService.Save(x.Sender));
        }

        public IObservable<IChangeSet<Field, int>> GetQuestions(int id) =>
            Observable.Create<IChangeSet<Field, int>>(observer =>
            {
                // API Call
                var clientDisposable = Observable.FromAsync(async () => await _apiClient.GetDocument(id)).Subscribe(_questions.AddOrUpdate);
                // Add new values akavache
                // Add values to _questions

                var questionDisposable = _questionChanges.Filter(x => x.Id == id).Subscribe(observer);

                return Disposable.Create(() =>
                {
                    clientDisposable.Dispose();
                    questionDisposable.Dispose();
                });
            });

        public void Save(Field field) => _cacheService.Save(field);
    }

    public interface ICacheService
    {
        void Save(Field field);
    }

    public interface IDocumentApiClient
    {
        Task<IEnumerable<Field>> GetDocument(int id);
    }
}