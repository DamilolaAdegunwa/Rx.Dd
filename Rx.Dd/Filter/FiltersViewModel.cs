using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Rocket.Surgery.Airframe.ViewModels;
using Rx.Dd.Data;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Rx.Dd.Filter
{
    public class FiltersViewModel  : NavigableViewModelBase
    {
        private readonly ReadOnlyObservableCollection<string> _alignments;
        private readonly ReadOnlyObservableCollection<Hero> _heroes;
        private readonly IHeroCache _heroCache;

        public FiltersViewModel(IHeroCache heroCache)
        {
            _heroCache = heroCache;

            heroCache
               .Transform(x => x.Alignment)
               .DistinctValues(x => x)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _alignments)
               .DisposeMany()
               .Subscribe(_ => { }, RxApp.DefaultExceptionHandler.OnNext)
               .DisposeWith(Garbage);

            static Func<Hero, bool> SelectAlignment(string alignment) => hero =>
            {
                if (!string.IsNullOrEmpty(alignment))
                {
                    return hero.Alignment == alignment;
                }

                return true;
            };

            heroCache
               .Filter(this.WhenAnyValue(x => x.SelectedAlignment).Select(SelectAlignment))
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _heroes)
               .DisposeMany()
               .Subscribe(_ => { }, RxApp.DefaultExceptionHandler.OnNext)
               .DisposeWith(Garbage);
        }

        public ReadOnlyObservableCollection<string> Alignments => _alignments;

        public ReadOnlyObservableCollection<Hero> Heroes => _heroes;

        [Reactive] public string SelectedAlignment { get; set; }

        protected override IObservable<Unit> ExecuteInitialize() => _heroCache.Load();
    }
}