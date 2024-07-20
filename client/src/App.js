import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';
import './styles/media.css';
import AddressForm from './AddressForm.js';
import MainPage from './MainPage';

const App = () => {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<AddressForm />} />
        <Route path="/main" element={<MainPage />} />
      </Routes>
    </Router>
  );
};

export default App;
