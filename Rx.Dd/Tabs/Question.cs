using DynamicData.Binding;

namespace Rx.Dd.Tabs
{
    public class Question : AbstractNotifyPropertyChanged
    {
        private string _questionText;
        private string _answerText;
        private bool _isCompleted;
        private int _id;
        private QuestionType _questionType;
        private bool _isMandatory;

        public string QuestionText
        {
            get => _questionText;
            set => SetAndRaise(ref _questionText, value);
        }

        public string AnswerText
        {
            get => _answerText;
            set => SetAndRaise(ref _answerText, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetAndRaise(ref _isCompleted, value);
        }

        public bool IsMandatory
        {
            get => _isMandatory;
            set => SetAndRaise(ref _isMandatory, value);
        }

        public int Id
        {
            get => _id;
            set => this.SetAndRaise(ref _id, value);
        }

        public QuestionType QuestionType
        {
            get => _questionType;
            set => this.SetAndRaise(ref _questionType, value);
        }
    }
}