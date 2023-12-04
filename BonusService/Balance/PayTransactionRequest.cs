using BonusService.Common.Postgres.Entity;
using Mediator;
namespace BonusService.Balance;

public sealed record PayTransactionRequest(Transaction transaction) : ICommand<long>;
