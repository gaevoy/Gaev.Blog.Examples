using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Gaev.Blog.Examples
{
    public class OtoDomCrawler
    {
        public async Task<List<Page>> Search(string url)
        {
            var offers = new List<Page>();
            while (url != null)
            {
                var html = (await new HtmlWeb().LoadFromWebAsync(url)).DocumentNode;
                offers.AddRange(ExtractOffers(html).Where(e => e.Link != null));
                url = GetNextUrl(html);
            }
            return offers;
        }

        private IEnumerable<Page> ExtractOffers(HtmlNode html)
        {
            var offers = html.SelectNodes("//*[starts-with(@class, 'offer-item-details')]");
            foreach (var offer in offers)
            {
                var title = offer.SelectSingleNode(".//*[starts-with(@class, 'offer-item-title')]");
                var price = offer.SelectSingleNode(".//*[starts-with(@class, 'offer-item-price')]");
                var link = title?.AncestorsAndSelf("a").FirstOrDefault();
                yield return new Page
                {
                    Link = link?.GetAttributeValue("href", null)?.Split("#")?.FirstOrDefault(),
                    Title = title?.InnerText + " " + price?.InnerText?.Trim()
                };
            }
        }

        private string GetNextUrl(HtmlNode html)
        {
            return html.SelectSingleNode("//a[@data-dir='next']")?.GetAttributeValue("href", null);
        }
    }
}