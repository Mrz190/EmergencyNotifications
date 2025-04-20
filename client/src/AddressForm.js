import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import config from './config';


const AddressForm = () => {
  const [credentials, setCredentials] = useState({
    userName: '',
    password: '',
  });
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('Token');
    if (token) {
      fetch(`${config.apiBaseUrl}/api/Auth/validate-jwt`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
      })
        .then(response => {
          if (response.ok) {
            navigate('/main');
          } else {
            setError('Token validation failed');
            localStorage.removeItem('Token');
          }
        })
        .catch(() => {
          console.error('Token validation error');
          localStorage.removeItem('Token');
          console.clear();
        });
    }
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setCredentials({ ...credentials, [name]: value });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    fetch(`${config.apiBaseUrl}/api/Auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    })
      .then(response => {
        if (!response.ok) {
          if (response.status === 400) {
            return response.json().then(data => {
              throw new Error(data.message || 'Invalid login credentials');
            });
          }
          throw new Error('Network response was not ok');
        }
        return response.json();
      })
      .then(data => {
        console.log('Credentials submitted:', data);
        const token = data["Token"];
        localStorage.setItem('Token', token);
        console.log('JWT token saved:', token);
        navigate('/main'); // Navigate to the main page
      })
      .catch(error => {
        setError(error.message); // Set the error message
        console.error('There was an error submitting the credentials!', error);

        const timeoutId = setTimeout(() => {
          setError('');
        }, 4500);

        return () => clearTimeout(timeoutId);
      });
  };

  const navToRegPage = () => {
    navigate("/register");
  }

  return (
    <header className="auth_wrapper">
      {error && <div className="error_label">Incorrect username or password.</div>}
      <h2>Emergency Notifications</h2>
      <h4 className="login_header">Login</h4>
      <form onSubmit={handleSubmit} className="auth_form">
        <br/>
        <div>
          <input className="input_data _login"
            autoComplete="off"
            placeholder="Username:"
            type="text"
            name="userName"
            value={credentials.userName}
            onChange={handleChange}
            required
          />
        </div>
        <div>
          <input className="input_data _password"
            autoComplete="off"
            placeholder="Password:"
            type="password"
            name="password"
            value={credentials.password}
            onChange={handleChange}
            required
          />
        </div>
        <button className="button-82-pushable" role="button" type="submit">
          <span className="button-82-shadow"></span>
          <span className="button-82-edge"></span>
          <span className="button-82-front text">
            Sign in &#8594;
          </span>
        </button>
        <br/>
      </form>
      <button className="auth_link" onClick={navToRegPage}>No account?</button>
    </header>
);
};

export default AddressForm;
