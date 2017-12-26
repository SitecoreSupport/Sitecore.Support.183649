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
      if (args == null)
      {
        throw new ArgumentNullException("args");
      }
      ChangeEventReason changeEventReason = this.GetChangeEventReason(args);
      IContact contact = args.GetContact();
      if (contact != null)
      {
        yield return new ContactChangeIndexable(new ContactIndexable(contact), changeEventReason);
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
          yield return new ContactChangeIndexable(new IndexableUniqueId<Guid>(args.ContactId.Guid), changeEventReason);
        }
      }
      yield break;
    }
  }
}
