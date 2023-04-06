using System;
using System.Threading.Tasks;

namespace onboard.util;

public class Result<T, E> {
    private class ResultException : Exception {
        public ResultException(string message) : base(message) { }
    }
    
    private T value { get; }
    private E error { get; }
    private bool _ok { get; }

    private Result(T value) {
        this.value = value;
        this.error = default(E);
        this._ok = true;
    }

    private Result(E error) {
        this.value = default(T);
        this.error = error;
        this._ok = false;
    }

    /// <summary>
    /// Returns true if the result is ok, false if it is an error.
    /// </summary>
    /// <returns></returns>
    public bool is_ok() {
        return this._ok;
    }

    /// <summary>
    /// Returns true if the result is ok and matches the predicate, false otherwise.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool is_ok_and(Func<T, bool> predicate) {
        return this._ok && predicate(this.value);
    }

    /// <summary>
    /// Returns true if the result is an error, false if it is ok.
    /// </summary>
    /// <returns></returns>
    public bool is_err() {
        return !this._ok;
    }

    /// <summary>
    /// Returns true if the result is an error and matches the predicate, false otherwise.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool is_err_and(Func<E, bool> predicate) {
        return !this._ok && predicate(this.error);
    }

    /// <summary>
    /// Returns Some T if the result is Ok, otherwise returns None
    /// </summary>
    /// <returns></returns>
    public Option<T> ok() {
        return this.is_ok() ? Option<T>.Some(value) : Option<T>.None();
    }

    /// <summary>
    /// Returns some E if the result is Err, otherwise returns None
    /// </summary>
    /// <returns></returns>
    public Option<E> err() {
        return this.is_err() ? Option<E>.Some(error) : Option<E>.None();
    }

    /// <summary>
    /// Returns a new result with Ok Type U while leaving errors unchanged
    /// </summary>
    /// <param name="map">a function transforming T into U</param>
    /// <typeparam name="U">Result Ok Type</typeparam>
    /// <returns></returns>
    public Result<U, E> map<U>(Func<T, U> map) {
        return this.is_ok() ? new Result<U, E>(map(this.value)) : new Result<U, E>(this.error);
    }

    /// <summary>
    /// Returns a new result with Ok Type U and Err Type F
    /// </summary>
    /// <param name="def">a function mapping Err E into Err F</param>
    /// <param name="map">a function mapping Ok T into Ok U</param>
    /// <typeparam name="U">Result Ok Type</typeparam>
    /// <typeparam name="F">Result Err Type</typeparam>
    /// <returns></returns>
    public Result<U, F> map_or<U, F>(Func<E, F> def, Func<T, U> map) {
        return this.is_ok() ? new Result<U, F>(map(this.value)) : new Result<U, F>(def(this.error));
    }

    /// <summary>
    /// Returns an object of Type U regardless of Ok or Err
    /// </summary>
    /// <param name="def">A function mapping Ok T to U</param>
    /// <param name="map">A function mapping Err E to U</param>
    /// <typeparam name="U">Return Type</typeparam>
    /// <returns></returns>
    public U map_or_else<U>(Func<E, U> def, Func<T, U> map) {
        return this.is_ok() ? map(this.value) : def(this.error);
    }

    /// <summary>
    /// Returns a new result with Err Type F, leaving Ok values unchanged
    /// </summary>
    /// <param name="map"></param>
    /// <typeparam name="F"></typeparam>
    /// <returns></returns>
    public Result<T, F> map_err<F>(Func<E, F> map) {
        return this.is_ok() ? new Result<T, F>(this.value) : new Result<T, F>(map(this.error));
    }

    /// <summary>
    /// Performs an action on an Ok result iff the Result is Ok
    /// </summary>
    /// <param name="inspect"></param>
    /// <returns></returns>
    public Result<T, E> inspect(Action<T> inspect) {
        if (this.is_ok()) {
            inspect(this.value);
        }

        return this;
    }

    /// <summary>
    /// Performs an action on an Err result iff the Result is Err
    /// </summary>
    /// <param name="inspect"></param>
    /// <returns></returns>
    public Result<T, E> inspect_err(Action<E> inspect) {
        if (this.is_err()) {
            inspect(this.error);
        }

        return this;
    }

    /// <summary>
    /// Returns the Ok value and throws an exception otherwise
    /// Differs from unwrap in that you can add an exception message
    /// </summary>
    /// <param name="message">A string </param>
    /// <returns></returns>
    /// <exception cref="Exception">Thrown if the Result contains an Err</exception>
    public T expect(string message) {
        if (this.is_ok()) {
            return this.value;
        }

        throw new ResultException(message);
    }

    /// <summary>
    /// Returns the Ok value and throws an exception otherwise
    /// Used when a result will always be Ok
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ResultException">Thrown if Result contains an Err</exception>
    public T unwrap() {
        if (this.is_ok()) {
            return this.value;
        }

        throw new ResultException("called Result.unwrap() on an error value");
    }

    /// <summary>
    /// Returns the Ok value of the Result or the default value for the Result's type
    /// </summary>
    /// <returns></returns>
    public T unwrap_or_default() {
        return this.is_ok() ? this.value : default;
    }

    /// <summary>
    /// Returns the Err value of the Result or throws an exception if the Result is Ok
    /// Differs from unwrap_err in that you can add an exception message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="Exception">Thrown if the Result contains an Ok Value</exception>
    public E expect_err(string message) {
        if (this.is_err()) {
            return this.error;
        }

        throw new ResultException(message);
    }

    /// <summary>
    /// Returns the Err value of the Result or throws an exception if the Result is Ok
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">Thrown if the Result contains on Ok Value</exception>
    public E unwrap_err() {
        if (this.is_err()) {
            return this.error;
        }

        throw new ResultException("called Result.unwrap_err() on an ok value");
    }

    /// <summary>
    /// Returns the other result if this is Ok, otherwise return this
    /// </summary>
    /// <param name="res"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Result<U, E> and<U>(Result<U, E> res) {
        return this.is_ok() ? res : new Result<U, E>(this.error);
    }

    /// <summary>
    /// Returns the output of the map function if this is Ok, otherwise return a new Result with this's Err
    /// </summary>
    /// <param name="map"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public Result<U, E> and_then<U>(Func<T, Result<U, E>> map) {
        return this.is_ok() ? map(this.value) : new Result<U, E>(this.error);
    }

    /// <summary>
    /// Returns this result if it is Ok, otherwise return the other result
    /// </summary>
    /// <param name="res"></param>
    /// <typeparam name="F"></typeparam>
    /// <returns></returns>
    public Result<T, F> or<F>(Result<T, F> res) {
        return this.is_ok() ? new Result<T, F>(this.value) : res;
    }

    /// <summary>
    /// Returns the output of the map function if this is Err, otherwise return this.
    /// </summary>
    /// <param name="map"></param>
    /// <typeparam name="F"></typeparam>
    /// <returns></returns>
    public Result<T, F> or_else<F>(Func<E, Result<T, F>> map) {
        return this.is_ok() ? new Result<T, F>(this.value) : map(this.error);
    }

    /// <summary>
    /// Returns the result if this is Ok, otherwise return the default
    /// </summary>
    /// <param name="def"></param>
    /// <returns></returns>
    public T unwrap_or(T def) {
        return this.is_ok() ? this.value : def;
    }

    /// <summary>
    /// Returns the result if this is Ok, otherwise returns the output of the map
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public T unwrap_or_else(Func<E, T> map) {
        return this.is_ok() ? this.value : map(this.error);
    }

    /// <summary>
    /// Returns the Ok value whether or not it is valid, this causes undefined
    /// behavior if the Result is Err
    /// </summary>
    /// <returns></returns>
    public T unwrap_unchecked() {
        return this.value;
    }

    /// <summary>
    /// Returns the Err value whether or not it is valid, this causes undefined
    /// behavior if the Result is Ok
    /// </summary>
    /// <returns></returns>
    public E unwrap_err_unchecked() {
        return this.error;
    }

    /// <summary>
    /// Transpose a Result containing an Option to an Option containing a Result
    /// </summary>
    /// <returns></returns>
    public Option<Result<T, E>> transpose() {
        return this.is_ok() ? Option<Result<T, E>>.Some(new Result<T, E>(this.value)) : Option<Result<T, E>>.None();
    }

    /// <summary>
    /// Match the Result and return a value
    /// </summary>
    /// <param name="if_ok"></param>
    /// <param name="if_err"></param>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U match<U>(Func<T, U> if_ok, Func<E, U> if_err) {
        return this.is_ok() ? if_ok(this.value) : if_err(this.error);
    }

    /// <summary>
    /// Match the Result and execute a function
    /// </summary>
    /// <param name="if_ok"></param>
    /// <param name="if_err"></param>
    /// <typeparam name="U"></typeparam>
    /// <typeparam name="V"></typeparam>
    public void match_void<U, V>(Func<T, U> if_ok, Func<E, V> if_err) {
        if (this.is_ok()) {
            if_ok(this.value);
        }
        else {
            if_err(this.error);
        }
    }

    /// <summary>
    /// Wraps a value in an ok Result
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T, E> Ok(T value) {
        return new Result<T, E>(value);
    }

    /// <summary>
    /// Wraps an error in an err Result
    /// </summary>
    /// <param name="err"></param>
    /// <returns></returns>
    public static Result<T, E> Err(E err) {
        return new Result<T, E>(err);
    }

    /// <summary>
    /// Awaits a task contained in a Result and returns a new Result with the awaited value
    /// </summary>
    /// <param name="res"></param>
    /// <returns></returns>
    public static Result<T, Exception> awaitInner(Result<Task<T>, Exception> res) {
        return res.is_ok() && res.value.IsCompletedSuccessfully
            ? new Result<T, Exception>(res.value.Result)
            : new Result<T, Exception>(res.is_err() ? res.error : new Exception("Task did not complete successfully"));
    }
}
