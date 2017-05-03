using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Search.Models;
using Search.Services;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Search.Dialogs;
using Search.Azure.Services;
using Microsoft.Azure.Search.Models;

namespace PictureBot.Dialogs
{
    [Serializable]
    public class SearchIntroDialog : IDialog<object>
    {
        protected readonly SearchQueryBuilder QueryBuilder = new SearchQueryBuilder();
        private readonly ISearchClient searchClient;

        // Jen adding this to go around Autofac
        public SearchIntroDialog()
        {
            var mapper = new PictureMapper();
            searchClient = new AzureSearchClient(mapper);
        }

        public SearchIntroDialog(ISearchClient searchClient)
        {
            SetField.NotNull(out this.searchClient, nameof(searchClient), searchClient);
        }

        public Task StartAsync(IDialogContext context)
        {
            try
            {
                //context.PostAsync("HI daniel");
                //context.Wait(this.SelectTitle);

                context.Call(
        new SearchRefineDialog(
            this.searchClient,
            "emotion",
            this.QueryBuilder,
            prompt: "Hi! To get started, what kind of picture are you looking for?"),
        this.StartSearchDialog);
            }
            catch (Exception e)
            {
                throw;
            }

            return Task.CompletedTask;
        }

        public Task SelectTitle(IDialogContext context, IAwaitable<IMessageActivity> input)
        {
            context.Call(
                new SearchRefineDialog(
                    this.searchClient,
                    "emotion",
                    this.QueryBuilder,
                    prompt: "Hi! To get started, what kind of picture are you looking for?"),
                this.StartSearchDialog);
            return Task.CompletedTask;
        }

        public async Task StartSearchDialog(IDialogContext context, IAwaitable<string> input)
        {
            string title = await input;

            if (string.IsNullOrEmpty(title))
            {
                context.Done<object>(null);
            }
            else
            {
                context.Call(new SearchPictureDialog(this.searchClient, this.QueryBuilder), this.Done);
            }
        }

        public async Task Done(IDialogContext context, IAwaitable<IList<SearchHit>> input)
        {
            // TODO: update this for our data
            var selection = await input;

            if (selection != null && selection.Any())
            {
                string list = string.Join("\n\n", selection.Select(s => $"* {s.Title} ({s.Key})"));
                await context.PostAsync($"Done! For future reference, you selected these job listings:\n\n{list}");
            }

            this.QueryBuilder.Reset();
            context.Done<object>(null);
        }
    }
}
