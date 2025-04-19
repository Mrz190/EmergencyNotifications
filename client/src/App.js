import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';
import './styles/media.css';
import AddressForm from './AddressForm';
import MainPage from './MainPage';
import RegisterForm from './RegisterForm';

const App = () => {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<AddressForm />} />
        <Route path="/main" element={<MainPage />} />
        <Route path="/register" element={<RegisterForm/>} />
      </Routes>
    </Router>
  );
};

export default App;
