namespace BackendTest.Application.Interfaces;

public interface IEntityService<TApplicationModel>
{
	IAsyncEnumerable<TApplicationModel?> GetAll(CancellationToken token = default);

	ValueTask<TApplicationModel?> Get(long id, CancellationToken token = default);

	ValueTask<long?> Insert(TApplicationModel model, CancellationToken token = default);

	ValueTask<bool> Update(TApplicationModel model, CancellationToken token = default);

	ValueTask<bool> Delete(long id, CancellationToken token = default);
}
