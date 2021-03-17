# Couchbase.Extension.MultiOp

Provides extensions for ICouchbaseCollection to perform multiple similar operations in parallel. For example, getting 100 documents based on their keys or performing a bulk insert of 100,000 documents. It is optimized to maximize throughput by limiting the degree of parallelization and to returning exceptions separately for each operation.

## Getting Documents

```cs
var collection = bucket.DefaultCollection();

var results = await bucket.Get(new[] { "key-1", "key-2", "key-3"}).ToList();

foreach (var result in results)
{
    // result.Exception contains any Couchbase error, such as DocumentNotFoundException
    // result.Result contains the IGetResult

    var document = result.Result.ContentAs<DocType>();
}
```

## Updating Documents

```cs
var collection = bucket.DefaultCollection();

// Note: any IEnumerable<string, TValue> can be used, allowing
// operations to be streamed for very large operation sets.
var documents = new Dictionary<string, DocType> 
{
    ["key-1"] = new DocType() { /* ... */ },
    ["key-2"] = new DocType() { /* ... */ },
    ["key-3"] = new DocType() { /* ... */ }
}

var results = await bucket.Upsert(documents).ToList();

foreach (var result in results)
{
    // result.Exception contains any Couchbase error, such as DocumentNotFoundException
    // result.Result contains the IMutationResult

    var cas = result.Result.Cas;
}
```

## Observables

Note that all operations return an `IObservable<MultiOpResult>` or an `IObservable<MultiOpResult<T>>`. These observables may be subscribed to receive the results as the operations complete, rather than waiting to receive the results in bulk at the end.

```cs
var subscription = bucket.Get(keys).Subscribe(
    result => {
        // Action is triggered for each result
    },
    (Exception ex) => {
        // Action is triggered for any Framework exception
    }
)

// At some point later, you may dispose of the subscription to cancel the operations
subscription.Dispose();
```

> :info: While it is possible to simply `await` the returned observable, this will only return the last operation result once all operations are completed. Use `.ToList()` to convert to an observable that returns the list of all results.

> :warning: Operations are not begun until the returned observable is subscribed. It is
> important to trigger a subscription so that the operations execute. This may include
> awaiting the observable, calling `.ToTask()`, or calling  `.ToAsyncEnumerable()` and then enumerating.

### Helpful Patterns

These patterns may be helpful for working with the returned observables.

```cs
// To simply ensure that all operations succeed, without keeping the results
await bucket.Upsert(documents).EnsureSuccessfulAsync();

// To get a list of results
var results = await bucket.Upsert(documents).ToList();

// To provide a CancellationToken which stops operations
var results = await bucket.Upsert(documents).ToList().ToTask(cancellationToken);

// To convert the observable to an IAsyncEnumerable<T>
await foreach (var result in bucket.Upsert(documents).ToAsyncEnumerable().WithCancellation(cancellationToken))
{
    // Do something here with each result
}
```

## Exceptions

Exceptions are handling using two different models.

Exceptions which inherit from `CouchbaseException` are returned on the result object for each operation. They do not interfere with the completion of any other operations. Consumers must validate each `MultiOpResult` object to see if each operation succeeded or failed.

All other exceptions, such as low-level framework exceptions, will be returned as an error on the observable. This cancels all further operations. For cases where the observable is being awaited, this will cause the exception to bubble up to your method. You may catch these exceptions using a `try..catch` block.
