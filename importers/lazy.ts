type Mapper<T, K> = (t: T) => K;
type Predicate<T> = Mapper<T, boolean>;

class Lazy<T, V = T> implements Iterator<V>, Iterable<V>  {
    protected iterable: Iterator<T>;

    constructor(iterable: Iterator<T>,
        mapper?: Mapper<T, V>,
        predicate?: Predicate<T>) {

        this.iterable = iterable;

        if (mapper !== undefined) {
            this.next = () => {
                let item = this.iterable.next();

                return {
                    value: mapper(item.value),
                    done: item.done,
                }
            };
        }
        else if (predicate !== undefined) {
            this.next = () => {
                let item = this.iterable.next();
                while (!item.done && !predicate(item.value)) {
                    item = this.iterable.next();
                }

                return item as any;
            }
        }
        else {
            this.next = () => {
                return this.iterable.next() as any;
            }
        }
    }

    [Symbol.iterator](): Iterator<V> {
        return {
            next: () => {
                return this.next();
            }
        }
    }

    next: () => IteratorResult<V>;

    filter(predicate: Predicate<V>) {
        return new Lazy<V, V>(this, undefined, predicate);
    }

    map<R>(mapper: Mapper<V, R>) {
        return new Lazy<V, R>(this, mapper, undefined);
    }
}

