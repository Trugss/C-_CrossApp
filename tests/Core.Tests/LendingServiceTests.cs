using Core.Events;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class LendingServiceTests
{
    [Fact]
    public void IssueCopy_WhenAlreadyIssued_ThrowsDomainException_AndKeepsState()
    {
        var store = new InMemoryBookStore([TestData.Copy("C-1", isIssued: true)]);
        var service = new LendingService(store);
        var events = 0;
        service.Changed += (_, _) => events++;

        var ex = Assert.Throws<DomainException>(() => service.IssueCopy("C-1", "R-1"));

        Assert.Contains("уже видано", ex.Message);
        Assert.True(store.GetById("C-1")!.IsIssued);
        Assert.Equal(0, events);
    }

    [Fact]
    public void ReturnCopy_WhenNotIssued_ThrowsDomainException_AndRaisesNothing()
    {
        var store = new InMemoryBookStore([TestData.Copy("C-1")]);
        var service = new LendingService(store);
        var events = 0;
        service.Changed += (_, _) => events++;

        Assert.Throws<DomainException>(() => service.ReturnCopy("C-1"));

        Assert.False(store.GetById("C-1")!.IsIssued);
        Assert.Equal(0, events);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void IssueCopy_WithNonPositiveLoanDays_ThrowsArgumentOutOfRange_AndKeepsState(int loanDays)
    {
        var store = new InMemoryBookStore([TestData.Copy("C-1")]);
        var service = new LendingService(store);

        Assert.Throws<ArgumentOutOfRangeException>(() => service.IssueCopy("C-1", "R-1", loanDays));

        Assert.False(store.GetById("C-1")!.IsIssued);
    }

    [Fact]
    public void IssueCopy_WithUnknownId_ThrowsDomainException()
    {
        var service = new LendingService(new InMemoryBookStore());

        Assert.Throws<DomainException>(() => service.IssueCopy("C-404", "R-1"));
    }

    [Fact]
    public void AddBook_RaisesChangedExactlyOnce()
    {
        var service = new LendingService(new InMemoryBookStore());
        var events = new List<LibraryChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        var copy = service.AddBook("978-966-03-0001-1", "Кайдашева сім'я");

        var e = Assert.Single(events);
        Assert.Equal(LibraryChangeKind.BookAdded, e.Kind);
        Assert.Equal(copy.Id, e.CopyId);
    }

    [Fact]
    public void IssueThenReturn_RaisesEventsInOrder_WithReaderOnIssue()
    {
        var service = new LendingService(new InMemoryBookStore([TestData.Copy("C-1")]));
        var events = new List<LibraryChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.IssueCopy("C-1", "R-7", loanDays: 10);
        service.ReturnCopy("C-1");

        Assert.Equal([LibraryChangeKind.CopyIssued, LibraryChangeKind.CopyReturned], events.Select(e => e.Kind));
        Assert.Equal("R-7", events[0].ReaderId);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today.AddDays(10)), events[0].DueDate);
    }
}
