import { useState } from "react";
import useAuth from "../hooks/UseAuth";
import httpClient from "../api/httpClient";

export const AuthPage = ({ initialView = "login" }) => {
  const { login } = useAuth();
  const [isLogin, setIsLogin] = useState(initialView === "login");

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [favoriteColor, setFavoriteColor] = useState("0");
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    const url = isLogin ? "/api/auth/login" : "/api/auth/register";
    const payload = isLogin
      ? { emailOrUsername: username, password }
      : { username, email, password, favoriteColor: parseInt(favoriteColor) };

    try {
      // Usiamo Axios con la configurazione dei cookie integrata
      const response = await httpClient.post(url, payload);

      // Il backend risponde con l'oggetto { user: { id, username, email, favoriteColor } }
      // I token sono già stati iniettati nei cookie dal server .NET!
      login(response.data.user);
    } catch (err) {
      // Cattura direttamente il ProblemDetails formattato dall'intercettore
      setError(err.message);
    }
  };

  return (
    <div className="w-full max-w-md p-4 sm:p-6">
      <div className="bg-slate-900 border border-slate-800 rounded-2xl p-6 sm:p-8 shadow-xl">
        <h2 className="text-2xl font-bold text-center text-white mb-6">
          {isLogin ? "Accedi a Stargazers" : "Crea un Account"}
        </h2>

        {error && (
          <div className="bg-red-900/50 border border-red-500 text-red-200 text-sm p-3 rounded-lg mb-4 text-center">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Campo Username / Email per Login, solo Username per Register */}
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-1">
              {isLogin ? "Username o Email" : "Username"}
            </label>
            <input
              type="text"
              required
              className="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-2 text-white focus:outline-none focus:border-blue-500 transition-colors"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>

          {/* Campo Email - Solo per la Registrazione */}
          {!isLogin && (
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">
                Email
              </label>
              <input
                type="email"
                required
                className="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-2 text-white focus:outline-none focus:border-blue-500 transition-colors"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
          )}

          {/* Campo Password */}
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-1">
              Password
            </label>
            <input
              type="password"
              required
              className="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-2 text-white focus:outline-none focus:border-blue-500 transition-colors"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          {/* Campo Colore Preferito - Solo per la Registrazione */}
          {!isLogin && (
            <div>
              <label className="block text-sm font-medium text-slate-300 mb-1">
                Colore Preferito
              </label>
              <select
                className="w-full bg-slate-950 border border-slate-800 rounded-lg px-3 py-2 text-white focus:outline-none focus:border-blue-500 transition-colors"
                value={favoriteColor}
                onChange={(e) => setFavoriteColor(e.target.value)}
              >
                <option value="0">Rosso 🔴</option>
                <option value="1">Blu 🔵</option>
                <option value="2">Verde 🟢</option>
              </select>
            </div>
          )}

          <button
            type="submit"
            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 rounded-lg transition-colors mt-6"
          >
            {isLogin ? "Accedi" : "Registrati"}
          </button>
        </form>

        <div className="mt-6 text-center text-sm text-slate-400">
          {isLogin ? "Nuovo su Stargazers?" : "Hai già un account?"}
          <button
            onClick={() => {
              setIsLogin(!isLogin);
              setError("");
            }}
            className="text-blue-400 hover:underline ml-1 font-medium focus:outline-none"
          >
            {isLogin ? "Registrati ora" : "Accedi qui"}
          </button>
        </div>
      </div>
    </div>
  );
};
