using Xunit;
using System.Collections.Generic;
using GildedRoseKata;

namespace GildedRoseTests;

public class GildedRoseTest
{
    [Fact]
    public void OrdinaryItem_BeforeExpiry_DegradesByOne_AndKeepsName()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "+5 Dexterity Vest", SellIn = 10, Quality = 20 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal("+5 Dexterity Vest", items[0].Name);
        Assert.Equal(9, items[0].SellIn);
        Assert.Equal(19, items[0].Quality);
    }

    [Fact]
    public void OrdinaryItem_AtZeroQuality_AndZeroSellIn_StaysAtZeroQuality_AndKeepsName()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "+5 Dexterity Vest", SellIn = 0, Quality = 0 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal("+5 Dexterity Vest", items[0].Name);
        Assert.Equal(-1, items[0].SellIn);
        Assert.Equal(0, items[0].Quality);
    }

    // ORD-4: an ordinary item with small positive Quality drops by 2 after expiry, floored at 0.
    [Fact]
    public void OrdinaryItem_AfterExpiry_QualityNeverGoesNegative()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "Elixir of the Mongoose", SellIn = 0, Quality = 1 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal("Elixir of the Mongoose", items[0].Name);
        Assert.Equal(-1, items[0].SellIn);
        Assert.Equal(0, items[0].Quality);
    }

    // BP-10: backstage pass gains +2 when 10 or fewer days remain (Quality low enough to show the gain).
    [Fact]
    public void BackstagePass_TenDaysLeft_IncreasesByTwo()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 10, Quality = 20 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal(9, items[0].SellIn);
        Assert.Equal(22, items[0].Quality);
    }

    // BP-6: at exactly 6 days the pass still gains only +2 (code uses SellIn < 6 for the +3 tier).
    [Fact]
    public void BackstagePass_SixDaysLeft_IncreasesByTwo()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 6, Quality = 20 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal(5, items[0].SellIn);
        Assert.Equal(22, items[0].Quality);
    }

    // BP-5: backstage pass gains +3 when 5 or fewer days remain.
    [Fact]
    public void BackstagePass_FiveDaysLeft_IncreasesByThree()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "Backstage passes to a TAFKAL80ETC concert", SellIn = 5, Quality = 20 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal(4, items[0].SellIn);
        Assert.Equal(23, items[0].Quality);
    }

    // BRIE-4: Aged Brie starting at 49 gains twice after expiry but is capped at 50 in one step.
    [Fact]
    public void AgedBrie_AfterExpiry_CapsAtFifty()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "Aged Brie", SellIn = 0, Quality = 49 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal("Aged Brie", items[0].Name);
        Assert.Equal(-1, items[0].SellIn);
        Assert.Equal(50, items[0].Quality);
    }

    // SULF-3: Sulfuras with an unusual value is left untouched (no auto-correction to 80).
    [Fact]
    public void Sulfuras_WithUnusualValues_KeepsBothValuesUnchanged()
    {
        IList<Item> items = new List<Item>
        {
            new Item { Name = "Sulfuras, Hand of Ragnaros", SellIn = 5, Quality = 10 }
        };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Equal("Sulfuras, Hand of Ragnaros", items[0].Name);
        Assert.Equal(5, items[0].SellIn);
        Assert.Equal(10, items[0].Quality);
    }

    // OBJ-1: the same Item object passed in is mutated in place, not replaced.
    [Fact]
    public void UpdateQuality_MutatesExistingItemObjectsInPlace()
    {
        var vest = new Item { Name = "+5 Dexterity Vest", SellIn = 5, Quality = 10 };
        IList<Item> items = new List<Item> { vest };
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Same(vest, items[0]);
        Assert.Equal(4, vest.SellIn);
        Assert.Equal(9, vest.Quality);
    }

    // INV-EMPTY: updating an empty inventory does not throw.
    [Fact]
    public void UpdateQuality_WithEmptyInventory_DoesNotThrow()
    {
        IList<Item> items = new List<Item>();
        GildedRose app = new GildedRose(items);

        app.UpdateQuality();

        Assert.Empty(items);
    }
}