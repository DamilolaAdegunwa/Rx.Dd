using System.Collections.Generic;
using NSubstitute;
using ReactiveUI.Testing;
using Rx.Dd.Tabs;

namespace Rx.Dd.Tests
{
    internal sealed class TabViewModelFixture : IBuilder
    {
        private int _id;
        private IQuestionsService _questionService = Substitute.For<IQuestionsService>();

        public TabViewModelFixture()
        {
            var questions = new List<Question>{ new Question { QuestionText = "Question?"} };
            _questionService.GetQuestions(Arg.Any<int>()).Returns(questions);
        }

        public static implicit operator TabViewModel(TabViewModelFixture fixture) => fixture.Build();
        public TabViewModelFixture WithId(int id) => this.With(out _id, id);
        public TabViewModelFixture WithService(IQuestionsService questionService) => this.With(out _questionService, questionService);
        private TabViewModel Build() => new TabViewModel("", _id, _questionService);
    }
}