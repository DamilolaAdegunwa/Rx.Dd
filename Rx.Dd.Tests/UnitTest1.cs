using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using ReactiveUI;
using ReactiveUI.Testing;
using Rx.Dd.Tabs;
using Xunit;

namespace Rx.Dd.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void GivenTabViewModels_WhenConstructed_ThenListedInOrder()
        {
            // Given, When
            TabsHostViewModel sut = new TabsHostViewModelFixture();

            // When

            // Then
            sut.Tabs
                .Should()
                .SatisfyRespectively(tab => tab.TabId.Should().Be(0),
                    tab => tab.TabId.Should().Be(1));
        }

        [Fact]
        public void GivenQuestionsAnswered_WhenSomeAreComplete_ThenCompletenessChanges()
        {
            // Given
            TabsHostViewModel sut = new TabsHostViewModelFixture();

            // When
            sut.Tabs[0].Questions[0].IsCompleted = true;

            // Then
            sut.Tabs[0].CompletedQuestions.Should().Be(1);
        }

        [Fact]
        public void GivenQuestionsCompleted_WhenCompletenessChanges_ThenTabCompletenessChanges()
        {
            // Given
            TabsHostViewModel sut = new TabsHostViewModelFixture();

            // When
            sut.Tabs[0].Questions[0].IsCompleted = true;

            // Then
            sut.Tabs[0].CompletedQuestions.Should().Be(1);
        }

        [Fact]
        public void GivenQuestionsCompleted_WhenCompletenessChanges_ThenTabCompletenessDoesNotChange()
        {
            // Given
            TabsHostViewModel sut = new TabsHostViewModelFixture();

            // When
            sut.Tabs[0].Questions[0].IsCompleted = true;

            // Then
            sut.Tabs[1].CompletedQuestions.Should().Be(0);
        }

        [Fact]
        public void GivenQuestions_WhenAnswered_ThenCallSave()
        {
            // Given
            var testScheduler = new TestScheduler();
            RxApp.TaskpoolScheduler = testScheduler;
            var questionService = Substitute.For<IQuestionsService>();
            
            var questions = new List<Question>{ new Question { QuestionText = "Question?"} };
            
            var tabViewModels = new List<TabViewModel>
            {
                new TabViewModelFixture().WithId(1),
                new TabViewModelFixture().WithId(0)
            };
            questionService.GetQuestionCategories().Returns(tabViewModels);
            questionService.GetQuestions(Arg.Any<int>()).Returns(questions);
            TabsHostViewModel sut = new TabsHostViewModelFixture().WithService(questionService);

            // When
            sut.Tabs[0].Questions[0].AnswerText = "Text";
            testScheduler.AdvanceByMs(301);
            
            // Then
            questionService.Received(1).Save(Arg.Any<Question>());
        }

        [Fact]
        public void Given_WhenSelectedTabChanged_Then_CurrenFieldsInContext()
        {
            // Given
            var testScheduler = new TestScheduler();
            RxApp.TaskpoolScheduler = testScheduler;
            var documentService = Substitute.For<IDocumentService>();
            
            var questions = new List<Field>
            {
                new Field { Id = 1, TabId = 0, MessageText = "Question?"},
                new Field { Id = 2, TabId = 0, MessageText = "Question?"},
                new Field { Id = 3, TabId = 1, MessageText = "Question?"},
                new Field { Id = 4, TabId = 1, MessageText = "Question?"},
                new Field { Id = 5, TabId = 1, MessageText = "Question?"}
            };
    
            documentService.GetQuestions(Arg.Any<int>()).Returns(questions.AsObservableChangeSet(x => x.Id).Filter(x => x.TabId == 1));

            TabsHostViewModel sut = new TabsHostViewModelFixture().WithService(documentService);

            // When
            sut.SelectedTab = new TabDetail("", 1, 0d);
            
            // Then
            sut.CurrentFields
                .Should()
                .OnlyContain(x => x.TabId == 1);
        }
    }

    public class DocumentServiceTest
    {
        [Fact]
        public void GivenDocument_WhenAnswered_ThenSaved()
        {
            // Given
            var cache = Substitute.For<ICacheService>();
            DocumentService sut = new DocumentServiceFixture().WithService(cache);
            sut.GetQuestions(1)
                .Bind(out var questions)
                .Subscribe();

            // When
            questions[0].AnswerText = "Answer";
            questions[0].IsValid = true;

            // Then
            cache.Received(1).Save(Arg.Any<Field>());
        }
    }

    internal sealed class DocumentServiceFixture : IBuilder
    {
        private IDocumentApiClient _client;
        private ICacheService _cacheService;

        public DocumentServiceFixture()
        {
            _client = Substitute.For<IDocumentApiClient>();
            _client.GetDocument(Arg.Any<int>()).Returns(new List<Field> {new Field {Id = 1}});
        }
        
        public static implicit operator DocumentService(DocumentServiceFixture fixture) => fixture.Build();
        public DocumentServiceFixture WithClient(IDocumentApiClient client) => this.With(out _client, client);
        public DocumentServiceFixture WithService(ICacheService cacheService) => this.With(out _cacheService, cacheService);
        private DocumentService Build() => new DocumentService(_client, _cacheService);
    }
}