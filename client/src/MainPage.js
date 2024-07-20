import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ReactComponent as CloseIcon } from './img/close.svg';
import config from './config';

const MainPage = () => {
  const [credentials, setCredentials] = useState({
    Name: '',
    Phone: '',
    Email: '',
    CreatedAt: '',
    CreatedBy: ''
  });

  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [contacts, setContacts] = useState([]);
  const [isNotifyOpen, setIsNotifyOpen] = useState(false);
  const [isContactSelectionVisible, setIsContactSelectionVisible] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setCredentials({ ...credentials, [name]: value });
  };

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
          if (!response.ok) {
            navigate('/');
          }
        })
        .catch(() => {
          localStorage.removeItem('Token');
          navigate('/');
        });
    } else {
      navigate('/');
    }
  }, [navigate]);

  useEffect(() => {
    const token = localStorage.getItem('Token');
    if (token) {
      const fetchContacts = () => {
        fetch(`${config.apiBaseUrl}/api/Contact/contacts-list`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
        })
          .then(response => response.json())
          .then(data => {
            setContacts(data);
          })
          .catch(error => {
            console.error('There was an error fetching the contacts!', error);
          });
      };

      fetchContacts();
      const intervalId = setInterval(fetchContacts, 1000);

      return () => clearInterval(intervalId);
    }
  }, []);

  const delAction = (id) => {
    debugger
    const token = localStorage.getItem('Token');
    if (token) {
      fetch(`${config.apiBaseUrl}/api/Contact/${id}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
      })
        .then(response => {
          if (!response.ok) {
            throw new Error('Network response was not ok');
          }
          setContacts(contacts.filter(contact => contact.id !== id));

          setSuccess('Contact deleted successfully');
          setError('');
          setTimeout(() => {
            setSuccess('');
          }, 4500);
        })
        .catch(error => {
          console.error('There was an error deleting the contact!', error);
        });
    }
  };

  const NotifyOpenField = () => {
    setIsNotifyOpen(true);
  };

  const handleMouseLeave = () => {
    setIsNotifyOpen(false);
  };

  const handleNotifyButtonClick = () => {
    setIsNotifyOpen(false);
    setIsContactSelectionVisible(false);
  };

  const NotifyUsers = () => {
    debugger
    console.log("notify");
  }

  const handleSubmit = (e) => {
    e.preventDefault();
    const token = localStorage.getItem('Token');
    if (token) {
      fetch(`${config.apiBaseUrl}/api/Contact/add-contact`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(credentials),
      })
        .then(response => {
          if (!response.ok) {
            if (response.status === 400 || response.status === 404) {
              setError('Contact with these credentials already exists.');
            }
            throw new Error('Network response was not ok');
          }
          return response.json();
        })
        .then(() => {
          setSuccess('Contact added successfully!');
          setError('');
          setTimeout(() => {
            setSuccess('');
          }, 4500);
          fetch(`${config.apiBaseUrl}/api/Contact/get-contacts`, {
            method: 'GET',
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${token}`
            },
          })
            .then(response => response.json())
            .then(data => {
              console.log("Fetched contacts after adding new one:", data);
              setContacts(data);
            })
            .catch(error => {
              console.error('There was an error fetching the contacts!', error);
            });
        })
        .catch(error => {
          console.error('There was an error submitting the credentials!', error);
          setError('There was an error submitting the credentials!');
          setSuccess('');
          setTimeout(() => {
            setError('');
          }, 4500);
        });
    }
  };

  return (
    <div className="global_wrapper">
      <header>
        <h1 className="header_name">Emergency Notification</h1>
        {error && <div className="error_label_add">{error}</div>}
        {success && <div className="success_label_add">{success}</div>}
      </header>

      <div className="main_container">
        <div className="add_contacts_field">
          <span>A<br />D<br />D<br /><br />C<br />O<br />N<br />T<br />A<br />C<br />T<br /></span>
          <div className="hidden_add_block">
            <h2>ADD NEW CONTACT</h2>
            <form onSubmit={handleSubmit} className="add_contact_form">
              <input className="input_contact _contact_name" autoComplete="off" placeholder="Name:" type="text" name="Name" value={credentials.Name} onChange={handleChange} required />
              <input className="input_contact _contact_phone" autoComplete="off" placeholder="Phone:" type="text" name="Phone" value={credentials.Phone} onChange={handleChange} required />
              <input className="input_contact _contact_email" autoComplete="off" placeholder="Email:" type="email" name="Email" value={credentials.Email} onChange={handleChange} required />
              <button className="btn_create_contact">Create contact</button>
            </form>
          </div>
        </div>
        <div className="wrapper_notify_btn_1">
          <div className="wrapper_notify_btn_2">
            <div className="wrapper_notify_btn_3">
              <div className="wrapper_notify_btn_4">
                <button className="pull_send_notoficate-pushable" role="button" type="button" onClick={NotifyOpenField}>
                  <span className="pull_send_notoficate-shadow"></span>
                  <span className="pull_send_notoficate-edge"></span>
                  <span className="pull_send_notoficate-front text">NOTIFY</span>
                </button>
              </div>
            </div>
          </div>
        </div>
        <div className="edit_contacts_field">
          <span>C<br />O<br />N<br />T<br />A<br />C<br />T<br />S<br /></span>
          <div className="hidden_edit_block">
            <h2>MY CONTACTS</h2>
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Phone</th>
                  <th>Email</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {contacts.map(contact => (
                  <tr key={contact.Id}>
                    <td>{contact.Name}</td>
                    <td>{contact.Phone}</td>
                    <td>{contact.Email}</td>
                    <td>
                      <button className="btn_del_contact" type="button" onClick={() => delAction(contact.Id)}><CloseIcon /></button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <div
        className={`contact_list_wrapper ${isNotifyOpen ? 'show' : ''}`}
        onMouseLeave={handleMouseLeave}
      >
        <div className="contact_list_field">
          <h1>NOTIFY USERS</h1><br />
          <form onSubmit={NotifyUsers} className="notify_form">
            <div className="message_area_notify">
              <textarea placeholder="Type message notification:" className="textarea_notify_message"></textarea>
            </div>
            <div className="contacts_area_notify">
              <h2>WHO TO NOTIFY?</h2>
              <div className="contacts_selection">
                {contacts.map(contact => (
                  <div key={contact.Id} className="checkbox-contacts">
                    <input type="checkbox" id={`contact-${contact.Id}`} name="notifyContacts" value={contact.Id} />
                    <label htmlFor={`contact-${contact.Id}`}>
                      <span></span>{contact.Name}
                    </label>
                  </div>
                ))}
              </div>
              <button type="submit" className="btn_send_notification">NOTIFY</button>
            </div>
          </form>
          <button className="notify_btn_close" onClick={handleNotifyButtonClick}>Close</button>
        </div>
      </div>
    </div>
  );
};

export default MainPage;
