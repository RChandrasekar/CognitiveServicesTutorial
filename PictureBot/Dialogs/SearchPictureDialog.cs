using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Search.Dialogs;
using Search.Models;
using Search.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PictureBot.Dialogs
{
    [Serializable]
    public class SearchPictureDialog: SearchDialog
    {
        //private static readonly string[] TopRefiners = { "business_title", "agency", "work_location" };
        private static readonly string[] TopRefiners = { "Tags", "TopEmotion", "Gender" };

        public SearchPictureDialog(ISearchClient searchClient, SearchQueryBuilder queryBuilder) : base(searchClient, queryBuilder, new JobStyler(), multipleSelection: true)
        {
        }

        protected override string[] GetTopRefiners()
        {
            return TopRefiners;
        }

        [Serializable]
        public class JobStyler : PromptStyler
        {
            public override void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, IReadOnlyList<string> descriptions = null)
            {
                // TODO: need to update this?  
                var hits = (IList<SearchHit>)options;

                var cards = hits.Select(h => new HeroCard
                {
                    Title = h.Title,
                    Buttons = new[] { new CardAction(ActionTypes.ImBack, "Save", value: h.Key) },
                    Text = h.Description
                });

                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = cards.Select(c => c.ToAttachment()).ToList();
                message.Text = prompt;
            }
        }
    }
}

