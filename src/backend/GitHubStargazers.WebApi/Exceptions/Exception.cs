namespace GitHubStargazers.WebApi.Exceptions;

public class EmailAlreadyInUseException(string email) : Exception($"The email '{email}' is already registered.") {}
public class InvalidCredentialsException() : Exception("Invalid username/email or password.") {}

