using MongoDB.Driver;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System;

namespace Mensch.Id.API.Helpers
{
    public static class AsyncHelpers
    {
        public static async IAsyncEnumerable<TProjection> ToAsyncEnumerable<TDocument,TProjection>(
            this IFindFluent<TDocument,TProjection> findFluent,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (findFluent == null) throw new ArgumentNullException(nameof(findFluent));

            var cursor = await findFluent.ToCursorAsync(cancellationToken);
            await foreach (var item in cursor.ToAsyncEnumerable(cancellationToken))
                yield return item;
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
            this IAsyncCursor<T> cursor,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (cursor == null) throw new ArgumentNullException(nameof(cursor));

            try
            {
                while (await cursor.MoveNextAsync(cancellationToken))
                {
                    var batch = cursor.Current;
                    foreach (var item in batch)
                    {
                        yield return item;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                cursor.Dispose();
            }
        }
    }
}
