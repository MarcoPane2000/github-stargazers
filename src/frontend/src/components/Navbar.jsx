import { useState } from 'react';
import useAuth from '../hooks/UseAuth';

export const Navbar = ({ onSwitchPage, currentPage }) => {
  const { user, isAuthenticated, logout } = useAuth();
  const [isOpen, setIsOpen] = useState(false); // Per il menu mobile (hamburger)

  return (
    <nav className="bg-slate-900 text-white shadow-md sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo / Titolo */}
          <div className="flex-shrink-0 flex items-center cursor-pointer" onClick={() => onSwitchPage('home')}>
            <span className="text-xl font-bold bg-gradient-to-r from-blue-400 to-indigo-500 bg-clip-text text-transparent">
              GitHub Stargazers
            </span>
          </div>

          {/* Menu Desktop */}
          <div className="hidden md:flex items-center space-x-4">
            {isAuthenticated ? (
              <>
                <span className="text-slate-300">Ciao, <strong className="text-white">{user.username}</strong></span>
                <button onClick={logout} className="bg-red-600 hover:bg-red-700 px-4 py-2 rounded-lg text-sm font-medium transition-colors">
                  Logout
                </button>
              </>
            ) : (
              <div className="space-x-2">
                <button onClick={() => onSwitchPage('login')} className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${currentPage === 'login' ? 'bg-blue-600' : 'hover:bg-slate-800'}`}>
                  Accedi / Registrati
                </button>
              </div>
            )}
          </div>

          {/* Bottone Hamburger Mobile */}
          <div className="md:hidden">
            <button onClick={() => setIsOpen(!isOpen)} className="text-slate-400 hover:text-white focus:outline-none">
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                {isOpen ? <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" /> : <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16" />}
              </svg>
            </button>
          </div>
        </div>
      </div>

      {/* Menu Dropdown Mobile */}
      {isOpen && (
        <div className="md:hidden bg-slate-800 px-2 pt-2 pb-3 space-y-1 sm:px-3 border-t border-slate-700">
          {isAuthenticated ? (
            <div className="flex flex-col space-y-2 p-2">
              <span className="text-slate-300 text-sm">Loggato come: <strong className="text-white">{user.username}</strong></span>
              <button onClick={() => { logout(); setIsOpen(false); }} className="w-full bg-red-600 hover:bg-red-700 px-4 py-2 rounded-lg text-sm font-medium text-center">
                Logout
              </button>
            </div>
          ) : (
            <button onClick={() => { onSwitchPage('login'); setIsOpen(false); }} className="w-full text-left block px-3 py-2 rounded-md text-base font-medium hover:bg-slate-700">
              Accedi / Registrati
            </button>
          )}
        </div>
      )}
    </nav>
  );
};