using System;

namespace onboard.util;

public class Option<T> {
    private class OptionException : Exception {
        public OptionException(string message) : base(message) { }
    }
    
    private T value;
    private bool hasValue;

    public static Option<T> Some(T value) {
        return new Option<T>(value);
    }

    public static Option<T> None() {
        return new Option<T>();
    }

    private Option(T value) {
        this.value = value;
        hasValue = true;
    }

    private Option() {
        this.value = default;
        hasValue = false;
    }

    /// <summary>
    /// Returns true if the option contains a value, false otherwise.
    /// </summary>
    /// <returns></returns>
    public bool is_some() {
        return hasValue;
    }

    /// <summary>
    /// Returns true iff the option contains a value that matches the given predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool is_some_and(Func<T, bool> predicate) {
        return is_some() && predicate(value);
    }

    /// <summary>
    /// Returns true if the option is None
    /// </summary>
    /// <returns></returns>
    public bool is_none() {
        return !hasValue;
    }

    /// <summary>
    /// Returns the contained value or throws an exception with the specified message if the option is None.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T expect(string message) {
        if (is_some()) return value;
        throw new OptionException(message);
    }

    /// <summary>
    /// Returns the contained value or throws an exception if the option is None.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T unwrap() {
        if (hasValue) {
            return value;
        }

        throw new OptionException("Called Unwrap on a None value");
    }

    /// <summary>
    /// Returns the contained value or the default value if the option is None.
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public T unwrap_or(T def) {
        return is_some() ? value : def;
    }

    /// <summary>
    /// Returns the contained value or the result of the function if the option is None.
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public T unwrap_or_else(Func<T> def) {
        return is_some() ? value : def();
    }

    /// <summary>
    /// Returns the contained value or the default value if the option is None.
    /// </summary>
    /// <returns></returns>
    public T unwrap_or_default() {
        return is_some() ? value : default;
    }

    /// <summary>
    /// Returns the contained value whether or not it is valid. This causes undefined behavior if the option is None.
    /// </summary>
    /// <returns></returns>
    public T unwrap_unchecked() {
        return value;
    }

    /// <summary>
    /// Returns an option containing the result of applying the given function to the contained value if the option is Some.
    /// </summary>
    /// <param name="map"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Option<U> map<U>(Func<T, U> map) {
        return is_some() ? Option<U>.Some(map(value)) : Option<U>.None();
    }

    /// <summary>
    /// Applies the given function to the contained value if the option is Some, then returns this.
    /// </summary>
    /// <param name="inspect"></param>
    /// <returns></returns>
    public Option<T> inspect(Action<T> inspect) {
        if (is_some()) {
            inspect(value);
        }

        return this;
    }

    /// <summary>
    /// Returns the map of the given function to the contained value if the option is Some, otherwise returns the default value.
    /// </summary>
    /// <param name="def"></param>
    /// <param name="map"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U map_or<U>(U def, Func<T, U> map) {
        return is_some() ? map(value) : def;
    }

    /// <summary>
    /// Returns the map of the given function to the contained value if the option is Some, otherwise returns the result of the default function.
    /// </summary>
    /// <param name="def"></param>
    /// <param name="map"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U map_or_else<U>(Func<U> def, Func<T, U> map) {
        return is_some() ? map(value) : def();
    }

    /// <summary>
    /// Returns a result containing the contained value if the option is Some, otherwise returns a result contianing the given error.
    /// </summary>
    /// <param name="e"></param>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public Result<T, E> ok_or<E>(E e) {
        return is_some() ? Result<T, E>.Ok(value) : Result<T, E>.Err(e);
    }

    /// <summary>
    /// Returns a result containing the contained value if the option is Some, otherwise returns a result containing the result of the given error function.
    /// </summary>
    /// <param name="f"></param>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public Result<T, E> ok_or_else<E>(Func<E> f) {
        return is_some() ? Result<T, E>.Ok(value) : Result<T, E>.Err(f());
    }

    /// <summary>
    /// Returns the other option if this option is Some, otherwise returns None.
    /// </summary>
    /// <param name="other"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Option<U> and<U>(Option<U> other) {
        return is_some() ? other : Option<U>.None();
    }

    /// <summary>
    /// Returns the result of the given function if this option is Some, otherwise returns None.
    /// </summary>
    /// <param name="f"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Option<U> and_then<U>(Func<T, Option<U>> f) {
        return is_some() ? f(value) : Option<U>.None();
    }

    /// <summary>
    /// Returns an option containing the contained value if the option is Some and the given predicate returns true, otherwise returns None.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public Option<T> filter(Func<T, bool> predicate) {
        return is_some() && predicate(value) ? this : None();
    }

    /// <summary>
    /// Returns the other option if this option is None, otherwise returns this.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Option<T> or(Option<T> other) {
        return is_some() ? this : other;
    }

    /// <summary>
    /// Returns the result of the given function if this option is None, otherwise returns this.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public Option<T> or_else(Func<Option<T>> f) {
        return is_some() ? this : f();
    }

    /// <summary>
    /// Returns None if this and the other option are both Some or both None, otherwise returns this.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Option<T> xor(Option<T> other) {
        return is_some() != other.is_some() ? this : None();
    }

    /// <summary>
    /// Sets the value of this option to the given value and returns that value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public T insert(T value) {
        this.value = value;
        hasValue = true;
        return value;
    }

    /// <summary>
    /// Sets the value of this option if the option is None,
    /// then returns the value of the option.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public T get_or_insert(T value) {
        if (!is_some()) return this.value;
        this.value = value;
        hasValue = true;
        return this.value;
    }

    /// <summary>
    /// Sets the value of this option to the default value if the option is None,
    /// then returns the value of the option.
    /// </summary>
    /// <returns></returns>
    public T get_or_insert_default() {
        if (!is_some()) return this.value;
        this.value = default;
        hasValue = true;
        return this.value;
    }

    /// <summary>
    /// Sets the value of this option to the result of the function if the option is None,
    /// then returns the value of the option.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public T get_or_insert_with(Func<T> f) {
        if (!is_some()) return this.value;
        this.value = f();
        hasValue = true;
        return this.value;
    }

    /// <summary>
    /// Takes the value out of the option, leaving a None in its place.
    /// </summary>
    /// <returns></returns>
    public Option<T> take() {
        if (!is_some()) return None();
        hasValue = false;
        return this;
    }

    /// <summary>
    /// Sets the value of this option to the passed value, then returns the previous value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Option<T> replace(T value) {
        Option<T> old = (Option<T>)this.MemberwiseClone();
        this.value = value;
        hasValue = true;
        return old;
    }

    /// <summary>
    /// Returns true if the option is Some and contains the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Option<U> contains<U>(U value) {
        return is_some() && this.value.Equals(value) ? Option<U>.Some(value) : Option<U>.None();
    }

    /// <summary>
    /// Returns an option tuple containing both contained values if both are Some, otherwise None
    /// </summary>
    /// <param name="other"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Option<(T, U)> zip<U>(Option<U> other) {
        return is_some() && other.is_some() ? Option<(T, U)>.Some((value, other.value)) : Option<(T, U)>.None();
    }

    /// <summary>
    /// Returns an option containing a new type as the result of the given function if both options are Some, otherwise returns None.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="f"></param>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    public Option<R> zip_with<U, R>(Option<U> other, Func<T, U, R> f) {
        return is_some() && other.is_some() ? Option<R>.Some(f(value, other.value)) : Option<R>.None();
    }

    /// <summary>
    /// Unzips an option tuple into a tuple of options.
    /// </summary>
    /// <param name="other"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public static (Option<T>, Option<U>) unzip<U>(Option<(T, U)> other) {
        return other.is_some()
            ? (Option<T>.Some(other.value.Item1), Option<U>.Some(other.value.Item2))
            : (None(), Option<U>.None());
    }

    /// <summary>
    /// Flattens an option of an option into an option.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public static Option<T> flatten(Option<Option<T>> other) {
        return other.is_some() ? other.value : None();
    }

    /// <summary>
    /// Match the option and return a value
    /// </summary>
    /// <param name="if_some"></param>
    /// <param name="if_none"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U match<U>(Func<T, U> if_some, Func<U> if_none) {
        return is_some() ? if_some(value) : if_none();
    }
}