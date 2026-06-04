import { useState } from 'react';
import { AuthProvider } from './components/AuthProvider';
import useAuth from './hooks/UseAuth'; // Assicurati che coincida con il nome esatto del tuo file
import { Navbar } from './components/Navbar'; 
import { AuthPage } from './pages/AuthPage'; // <--- MANCAVA QUESTO IMPORT!
import { DashboardPage } from './pages/DashboardPage';

const AppContent = () => {
  const { isAuthenticated } = useAuth();
  const [authView, setAuthView] = useState('login'); // 'login' o 'register'

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans">
      <Navbar 
        onSwitchPage={(page) => setAuthView(page)} 
        currentPage={authView} 
      />
      
      <main className="flex-grow flex flex-col justify-center items-center w-full">
        {isAuthenticated ? (
          // Se è loggato, vede l'applicazione principale responsiva
          <DashboardPage />
        ) : (
          // FIX: Se non è loggato, vede la pagina dei Form, non il Provider!
          <AuthPage initialView={authView} />
        )}
      </main>
    </div>
  );
};

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}