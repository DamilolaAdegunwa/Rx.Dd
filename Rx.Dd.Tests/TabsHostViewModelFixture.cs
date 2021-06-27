using System.Collections.Generic;
using NSubstitute;
using ReactiveUI.Testing;
using Rx.Dd.Tabs;

namespace Rx.Dd.Tests
{
    internal sealed class TabsHostViewModelFixture : IBuilder
    {
        private IQuestionsService _questionsService;
        private IDocumentService _documentService;

        public TabsHostViewModelFixture()
        {
            _questionsService = Substitute.For<IQuestionsService>();
            var tabViewModels = new List<TabViewModel>
            {
                new TabViewModelFixture().WithId(1),
                new TabViewModelFixture().WithId(0)
            };
            _questionsService.GetQuestionCategories().Returns(tabViewModels);
        }
        public static implicit operator TabsHostViewModel(TabsHostViewModelFixture fixture) => fixture.Build();

        public TabsHostViewModelFixture WithService(IQuestionsService questionsService) =>
            this.With(out _questionsService, questionsService);
        
        public TabsHostViewModelFixture WithService(IDocumentService documentService) =>
            this.With(out _documentService, documentService);

        private TabsHostViewModel Build() => new TabsHostViewModel(_questionsService, _documentService);
    }
}