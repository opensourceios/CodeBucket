using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeBucket.Core.Filters;
using BitbucketSharp.Models;
using CodeBucket.Core.Utils;

namespace CodeBucket.Core.ViewModels.Repositories
{
    public abstract class RepositoriesViewModel : LoadableViewModel
    {
		private readonly FilterableCollectionViewModel<RepositoryDetailedModel, RepositoriesFilterModel> _repositories;

        public bool ShowRepositoryDescription
        {
			get { return this.GetApplication().Account.RepositoryDescriptionInList; }
        }

		public FilterableCollectionViewModel<RepositoryDetailedModel, RepositoriesFilterModel> Repositories
        {
            get { return _repositories; }
        }

        public ICommand GoToRepositoryCommand
        {
			get { return new MvxCommand<RepositoryDetailedModel>(x => this.ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner, RepositorySlug = x.Slug })); }
        }

        protected RepositoriesViewModel(string filterKey = "RepositoryController")
        {
			_repositories = new FilterableCollectionViewModel<RepositoryDetailedModel, RepositoriesFilterModel>(filterKey);
			_repositories.FilteringFunction = x => Repositories.Filter.Ascending ? x.OrderBy(y => y.Name) : x.OrderByDescending(y => y.Name);
            _repositories.GroupingFunction = CreateGroupedItems;
            _repositories.Bind(x => x.Filter).Subscribe(x => Repositories.Refresh());
        }

		private IEnumerable<IGrouping<string, RepositoryDetailedModel>> CreateGroupedItems(IEnumerable<RepositoryDetailedModel> model)
        {
            var order = Repositories.Filter.OrderBy;

            if (order == RepositoriesFilterModel.Order.LastUpdated)
            {
                var a = model.OrderByDescending(x => x.UtcLastUpdated).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UtcLastUpdated.TotalDaysAgo()));
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return FilterGroup.CreateNumberedGroup(a, "Days Ago", "Updated");
            }
            if (order == RepositoriesFilterModel.Order.CreatedOn)
            {
                var a = model.OrderByDescending(x => x.UtcCreatedOn).GroupBy(x => FilterGroup.IntegerCeilings.First(r => r > x.UtcCreatedOn.TotalDaysAgo()));
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return FilterGroup.CreateNumberedGroup(a, "Days Ago", "Created");
            }
            if (order == RepositoriesFilterModel.Order.Owner)
            {
                var a = model.OrderBy(x => x.Name).GroupBy(x => x.Owner);
                a = Repositories.Filter.Ascending ? a.OrderBy(x => x.Key) : a.OrderByDescending(x => x.Key);
                return a.ToList();
            }

            return null;
        }
    }
}