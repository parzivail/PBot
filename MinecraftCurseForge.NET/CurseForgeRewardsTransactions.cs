using System;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HtmlAgilityPack;

namespace MinecraftCurseForge.NET
{
	public class CurseForgeRewardsTransactions
	{
		public static CurseForgeRewardsTransaction[] Parse(string xml)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml("<transactions>" + xml + "</transactions>");

			return doc.DocumentNode.ChildNodes["transactions"].ChildNodes
				.Where(node => node.HasClass("transactions"))
				.Select(ParseTransaction)
				.Where(transaction => transaction != null)
				.ToArray();
		}

		private static HtmlNode SelectChildWithClass(HtmlNode node, string classNames)
		{
			if (node.GetAttributeValue("class", null) == classNames)
				return node;

			return node.ChildNodes
				.Select(childNode => SelectChildWithClass(childNode, classNames))
				.FirstOrDefault(foundChild => foundChild != null);
		}

		private static CurseForgeRewardsTransaction ParseTransaction(HtmlNode node)
		{
			var timestampAbbr = SelectChildWithClass(node, "tip standard-date standard-datetime");
			var timestampEpoch = timestampAbbr.GetAttributeValue("data-epoch", 0L);
			var timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampEpoch).DateTime;

			var awardDiv = SelectChildWithClass(node, "reward-item award");

			if (awardDiv == null)
				return null;

			var pointsText = awardDiv.SelectSingleNode("a/span");
			var points = pointsText.SelectSingleNode("strong").InnerText;

			var pointsBreakdownUl = awardDiv.SelectSingleNode("div/ul");
			var pointsBreakdown = pointsBreakdownUl.ChildNodes
				.Where(n => n.Name == "li")
				.Select(ParseBreakdownItem)
				.ToArray();

			return new CurseForgeRewardsTransaction(timestamp, (int)(decimal.Parse(points) * 100), pointsBreakdown);
		}

		private static CurseForgeRewardsTransactionBreakdownItem ParseBreakdownItem(HtmlNode node)
		{
			var points = node.SelectSingleNode("b").InnerText;

			var link = node.SelectSingleNode("a");
			var url = link.GetAttributeValue("href", null);
			var projectName = link.InnerText;

			return new CurseForgeRewardsTransactionBreakdownItem((int)(decimal.Parse(points) * 100), WebUtility.HtmlDecode(projectName), url);
		}
	}
}