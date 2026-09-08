using System.Collections.Generic;

namespace GildedRoseKata;

public class GildedRose
{
    private const string AgedBrie = "Aged Brie";
    private const string BackstagePass = "Backstage passes to a TAFKAL80ETC concert";
    private const string Sulfuras = "Sulfuras, Hand of Ragnaros";
    private const string ConjuredPrefix = "Conjured ";

    private const int MinQuality = 0;
    private const int MaxQuality = 50;

    // Backstage passes gain extra quality as the concert approaches:
    // an extra +1 when 10 or fewer days remain, and another +1 when 5 or fewer remain.
    private const int BackstageDoubleGainThreshold = 11; // SellIn < 11 means 10 or fewer days left
    private const int BackstageTripleGainThreshold = 6;  // SellIn < 6 means 5 or fewer days left

    IList<Item> Items;

    public GildedRose(IList<Item> Items)
    {
        this.Items = Items;
    }

    public void UpdateQuality()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (Items[i].Name == AgedBrie)
            {
                UpdateAgedBrie(Items[i]);
                continue;
            }

            if (Items[i].Name == BackstagePass)
            {
                UpdateBackstagePass(Items[i]);
                continue;
            }

            if (Items[i].Name == Sulfuras)
            {
                // Sulfuras is legendary: its Quality and SellIn never change.
                continue;
            }

            if (IsConjured(Items[i]))
            {
                UpdateConjuredItem(Items[i]);
                continue;
            }

            UpdateOrdinaryItem(Items[i]);
        }
    }

    // An item is Conjured when its name starts with "Conjured " (including the
    // trailing space), compared case-sensitively.
    private static bool IsConjured(Item item)
    {
        return item.Name != null
            && item.Name.StartsWith(ConjuredPrefix, System.StringComparison.Ordinal);
    }

    private static void UpdateConjuredItem(Item item)
    {
        // Conjured items degrade twice as fast as ordinary items:
        // by 2 before expiry and by 4 once past their sell date, never below 0.
        DecreaseQuality(item);
        DecreaseQuality(item);

        item.SellIn = item.SellIn - 1;

        if (item.SellIn < 0)
        {
            DecreaseQuality(item);
            DecreaseQuality(item);
        }
    }

    private static void UpdateOrdinaryItem(Item item)
    {
        // Ordinary items lose 1 quality per day, and 2 per day once past their sell date.
        DecreaseQuality(item);

        item.SellIn = item.SellIn - 1;

        if (item.SellIn < 0)
        {
            DecreaseQuality(item);
        }
    }

    private static void UpdateAgedBrie(Item item)
    {
        // Aged Brie increases in quality by 1 each day, and by 2 once past its sell date.
        IncreaseQuality(item);

        item.SellIn = item.SellIn - 1;

        if (item.SellIn < 0)
        {
            IncreaseQuality(item);
        }
    }

    private static void UpdateBackstagePass(Item item)
    {
        // Quality rises as the concert nears, then drops to zero once it has passed.
        IncreaseQuality(item);

        if (item.SellIn < BackstageDoubleGainThreshold)
        {
            IncreaseQuality(item);
        }

        if (item.SellIn < BackstageTripleGainThreshold)
        {
            IncreaseQuality(item);
        }

        item.SellIn = item.SellIn - 1;

        if (item.SellIn < 0)
        {
            item.Quality = MinQuality;
        }
    }

    private static void IncreaseQuality(Item item)
    {
        if (item.Quality < MaxQuality)
        {
            item.Quality = item.Quality + 1;
        }
    }

    private static void DecreaseQuality(Item item)
    {
        if (item.Quality > MinQuality)
        {
            item.Quality = item.Quality - 1;
        }
    }
}
