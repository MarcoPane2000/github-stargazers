import { useState, useEffect } from "react";
import httpClient from "../api/httpClient";

export const DashboardPage = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [suggestions, setSuggestions] = useState([]);
  const [repositories, setRepositories] = useState([]);
  const [stargazers, setStargazers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [history, setHistory] = useState([]);
  const [currentView, setCurrentView] = useState("search");

  useEffect(() => {
    if (searchTerm.trim().length < 3) {
      return;
    }

    const delayDebounceFn = setTimeout(async () => {
      try {
        const response = await httpClient.get(
          `/api/stargazers/users?q=${searchTerm}`,
        );
        setSuggestions(response.data);
      } catch (err) {
        console.error(err);
      }
    }, 400);

    return () => clearTimeout(delayDebounceFn);
  }, [searchTerm]);

  const handleSelectUser = async (username) => {
    if (!username) return;
    setSearchTerm(username);
    setSuggestions([]);
    setLoading(true);
    setError("");

    try {
      const response = await httpClient.get(`/api/stargazers/repos/${username}`);
      setRepositories(response.data);
      setCurrentView("user-profile");
    } catch (err) {
      setError(err.message);
      setRepositories([]);
    } finally {
      setLoading(false);
    }
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    if (searchTerm.trim()) {
      setSuggestions([]);
      handleSelectUser(searchTerm.trim());
    }
  };

  const handleNavigateToRepo = async (username, repoName) => {
    if (!username || !repoName) return;
    setSuggestions([]);
    setHistory((prev) => [
      ...prev,
      { view: currentView, searchTerm, repos: repositories, stargazers },
    ]);

    setLoading(true);
    setError("");
    try {
      const response = await httpClient.get(
        `/api/stargazers/repos/${username}/${repoName}/stargazers`,
      );
      setStargazers(response.data);
      setCurrentView("repo-stargazers");
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleGoBack = () => {
    setSuggestions([]);
    if (history.length === 0) {
      setCurrentView("search");
      return;
    }

    const previousState = history[history.length - 1];
    setHistory((prev) => prev.slice(0, -1));

    setCurrentView(previousState.view);
    setSearchTerm(previousState.searchTerm);
    setRepositories(previousState.repos);
    setStargazers(previousState.stargazers);
  };

  return (
    <div className="w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 flex flex-col items-center">
      {history.length > 0 && (
        <div className="w-full max-w-2xl flex justify-start mb-4">
          <button
            onClick={handleGoBack}
            className="flex items-center space-x-2 text-sm font-medium text-slate-400 hover:text-white transition-colors bg-slate-900 border border-slate-800 px-4 py-2 rounded-xl cursor-pointer"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            <span>Indietro</span>
          </button>
        </div>
      )}

      <div className="w-full max-w-2xl mb-12 relative z-50">
        <h1 className="text-3xl font-extrabold text-center text-white mb-2">
          Esplora i Repository stargazers
        </h1>
        <p className="text-slate-400 text-center text-sm sm:text-base mb-6">
          Digita un username per visualizzare i suggerimenti e analizzare i suoi progetti in tempo reale.
        </p>

        <form onSubmit={handleSearchSubmit} className="relative flex flex-col sm:flex-row gap-3 w-full">
          <div className="relative flex-grow">
            <input
              type="text"
              placeholder="Es: affaan-m o torvalds..."
              className="w-full bg-slate-900 border border-slate-800 rounded-xl px-4 py-3 text-white placeholder-slate-500 focus:outline-none focus:border-blue-500 transition-colors"
              value={searchTerm}
              onBlur={() => {
                setTimeout(() => setSuggestions([]), 200);
              }}
              onChange={(e) => {
                const value = e.target.value;
                setSearchTerm(value);
                if (value.trim().length < 3) {
                  setSuggestions([]);
                }
              }}
            />

            {suggestions.length > 0 && (
              <ul className="absolute left-0 right-0 mt-2 bg-slate-900 border border-slate-800 rounded-xl shadow-2xl overflow-hidden max-h-60 overflow-y-auto">
                {suggestions.map((user, idx) => {
                  const name = user.owner || user.login || `user-${idx}`;
                  return (
                    <li
                      key={name}
                      onClick={() => handleSelectUser(name)}
                      className="flex items-center space-x-3 px-4 py-3 hover:bg-slate-800 cursor-pointer transition-colors border-b border-slate-800/50 last:border-0"
                    >
                      <img
                        src={user.avatarUrl}
                        alt={name}
                        className="w-8 h-8 rounded-full border border-slate-700"
                      />
                      <span className="text-white font-medium">
                        {name}
                      </span>
                    </li>
                  );
                })}
              </ul>
            )}
          </div>

          <button
            type="submit"
            disabled={loading}
            className="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-800 text-white font-medium px-6 py-3 rounded-xl transition-colors shrink-0 cursor-pointer"
          >
            {loading ? "Ricerca..." : "Cerca"}
          </button>
        </form>
      </div>

      {error && (
        <div className="bg-red-900/30 border border-red-500/50 text-red-200 p-4 rounded-xl max-w-md w-full text-center mb-8">
          {error}
        </div>
      )}

      {currentView === "user-profile" && (
        <div className="w-full grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 relative z-10">
          {repositories.map((repo) => {
            const currentOwner = repo.ownerLogin || searchTerm;
            return (
              <div
                key={repo.repoName}
                onClick={() => handleNavigateToRepo(currentOwner, repo.repoName)}
                className="bg-slate-900 border border-slate-800 hover:border-blue-500/50 p-5 rounded-2xl flex flex-col justify-between shadow-md transition-all hover:-translate-y-1 cursor-pointer group"
              >
                <div>
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center space-x-2">
                      <span className="text-xs font-mono text-slate-500 bg-slate-800 px-2 py-1 rounded-md">
                        @{currentOwner}
                      </span>
                      <h3 className="text-lg font-bold text-white truncate max-w-[160px] group-hover:text-blue-400 transition-colors" title={repo.repoName}>
                        {repo.repoName}
                      </h3>
                    </div>
                    
                    <div className="flex items-center space-x-1 text-amber-400 bg-amber-400/10 px-2.5 py-1 rounded-lg border border-amber-400/20">
                      <svg className="w-4 h-4 fill-current" viewBox="0 0 20 20">
                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                      </svg>
                      <span className="text-sm font-bold">{repo.stargazersCount}</span>
                    </div>
                  </div>

                  <p className="text-slate-400 text-sm line-clamp-3 mb-4">
                    {repo.repoDescription}
                  </p>
                </div>

                <div className="flex items-center justify-between pt-3 border-t border-slate-800/60 text-xs text-slate-500 group-hover:text-blue-400/80 transition-colors">
                  <span>Vedi chi ha messo mi piace</span>
                  <svg className="w-4 h-4 transform group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {currentView === "repo-stargazers" && (
        <div className="w-full max-w-2xl bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-xl relative z-10">
          <h2 className="text-xl font-bold text-white mb-6 flex items-center space-x-2">
            <span>Utenti che hanno messo nei preferiti</span>
          </h2>
          <ul className="divide-y divide-slate-800">
            {stargazers.map((stargazer, idx) => {
              const userName = stargazer.owner || stargazer.login || `stargazer-${idx}`;
              return (
                <li
                  key={userName}
                  onClick={() => handleSelectUser(userName)}
                  className="flex items-center justify-between py-3 first:pt-0 last:pb-0 hover:bg-slate-800/30 px-2 rounded-xl transition-colors cursor-pointer"
                >
                  <div className="flex items-center space-x-3">
                    <img
                      src={stargazer.avatarUrl}
                      alt={userName}
                      className="w-10 h-10 rounded-full border border-slate-700"
                    />
                    <span className="text-white font-medium">
                      {userName}
                    </span>
                  </div>
                  <svg className="w-5 h-5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </li>
              );
            })}
          </ul>
          {stargazers.length === 0 && (
            <p className="text-center text-slate-500 py-4">
              Nessuno stargazer trovato per questo repository.
            </p>
          )}
        </div>
      )}

      {currentView === "search" && !loading && (
        <div className="text-center py-16 text-slate-500">
          <svg className="w-12 h-12 mx-auto mb-3 opacity-30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
          Nessun repository caricato. Digita e seleziona un utente sopra per esplorare i progetti.
        </div>
      )}
    </div>
  );
};