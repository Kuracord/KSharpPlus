using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace KSharpPlus; 

internal readonly struct ReadOnlyConcurrentDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> {
    readonly ConcurrentDictionary<TKey, TValue> _underlyingDict;

    public ReadOnlyConcurrentDictionary(ConcurrentDictionary<TKey, TValue> underlyingDict) => _underlyingDict = underlyingDict;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _underlyingDict.GetEnumerator();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_underlyingDict).GetEnumerator();

    public int Count => _underlyingDict.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) => _underlyingDict.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, out TValue value) => _underlyingDict.TryGetValue(key, out value);

    public TValue this[TKey key] => _underlyingDict[key];

    public IEnumerable<TKey> Keys => _underlyingDict.Keys;

    public IEnumerable<TValue> Values => _underlyingDict.Values;
}