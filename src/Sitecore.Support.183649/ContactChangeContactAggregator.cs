using System.Linq;
using System.Reflection;
using Sitecore.Configuration;
using Sitecore.ContentSearch.Analytics.Aggregators;
using Sitecore.ContentSearch.Security;

namespace Sitecore.Support.ContentSearch.Analytics.Aggregators
{
    using Sitecore.Analytics.Aggregation.Pipelines.ContactProcessing;
    using Sitecore.Analytics.Model.Entities;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Analytics.Extensions;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.ContentSearch.Diagnostics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class ContactChangeContactAggregator : ContactChangeProcessor<IContactIndexable>
    {
        public ContactChangeContactAggregator(string name) : base(name)
        {
        }

        protected override IEnumerable<IContactIndexable> ResolveIndexables(ContactProcessingArgs args)
        {
            Func<string> messageDelegate = null;
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            ChangeEventReason changeEventReason = this.GetChangeEventReason(args);
            IContact contact = args.GetContact();
            if (contact != null)
            {
                if (contact.ShouldBeIndexed())
                {
                    yield return new ContactChangeIndexable(new ContactIndexable(contact), changeEventReason);
                    goto Label_PostSwitchInIterator;
                }
                if (messageDelegate == null)
                {
                    messageDelegate = () => $"The contact will not be indexed because contact {contact.Id} does not have an identifier and the system is not configured to index anonymous contacts and their interactions.";
                }
                ObservationLog.Log.Debug(messageDelegate, null);
            }
            else
            {
                if (changeEventReason == ChangeEventReason.Deleted)
                {
                    //create empty contact with specified id.
                    IContactFactory factory = Sitecore.Diagnostics.Assert.ResultNotNull<IContactFactory>(Factory.CreateObject("model/entities/contact/factory", true) as IContactFactory);
                    contact = factory.Create(args.ContactId);
                    yield return new ContactChangeIndexable(
                        new Sitecore.ContentSearch.Analytics.Models.ContactIndexable(contact),
                        changeEventReason);
                }
                else
                {
                    yield return new ContactChangeIndexable(new IndexableUniqueId<Guid>(args.ContactId.Guid),
                        changeEventReason);
                }
                goto Label_PostSwitchInIterator;
            }
            yield break;
            Label_PostSwitchInIterator:;
        }
    }
}
