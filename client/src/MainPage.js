import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import config from './config';

const MainPage = () => {
  const [credentials, setCredentials] = useState({
    Name: '',
    Phone: '',
    Email: '',
    CreatedAt: '',
    CreatedBy: ''
  });

  const [editContact, setEditContact] = useState(null);
  const [isSending, setIsSending] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [contacts, setContacts] = useState([]);
  const [isNotifyOpen, setIsNotifyOpen] = useState(false);
  const [isContactSelectionVisible, setIsContactSelectionVisible] = useState(false);
  const [notificationMessageBody, setNotificationMessageBody] = useState('');
  const [selectedContacts, setSelectedContacts] = useState([]);
  const [isActionModalOpen, setIsActionModalOpen] = useState(null);
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setCredentials({ ...credentials, [name]: value });
  };

  const handleEditChange = (e) => {
    const { name, value } = e.target;
    setEditContact((prev) => ({ ...prev, [name]: value }));
  };

  const handleEditSubmit = (e) => {
    e.preventDefault();
    const token = localStorage.getItem('Token');
    if (token) {
      fetch(`${config.apiBaseUrl}/api/Contact/${editContact.Id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(editContact),
      })
        .then(response => {
          if (!response.ok) {
            throw new Error('Network response was not ok');
          }
          setSuccess('Contact updated successfully!');
          setError('');
          setTimeout(() => {
            setSuccess('');
          }, 4500);
          setIsActionModalOpen(null);
          setEditContact(null);
        })
        .catch(error => {
          console.error('There was an error updating the contact!', error);
          setError('There was an error updating the contact!');
          setSuccess('');
          setTimeout(() => {
            setError('');
          }, 4500);
        });
    }
  };

  const fetchContacts = () => {
    const token = localStorage.getItem('Token');
    if (token) {
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
    }
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
    fetchContacts();
    const intervalId = setInterval(fetchContacts, 1000);

    return () => clearInterval(intervalId);
  }, []);

  const delAction = (id) => {
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
          fetchContacts();
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

  const handleMessageBodyChange = (e) => {
    setNotificationMessageBody(e.target.value);
  };

  const handleContactSelection = (e) => {
    const { value, checked } = e.target;
    setSelectedContacts((prevSelectedContacts) =>
      checked ? [...prevSelectedContacts, value] : prevSelectedContacts.filter((id) => id !== value)
    );
  };

  const NotifyUsers = (e) => {
    e.preventDefault();
    setIsSending(true);
    setIsSuccess(false);

    const token = localStorage.getItem('Token');

    if (token) {
      const recipients = contacts
        .filter(contact => selectedContacts.includes(contact.Id.toString()))
        .map(contact => ({ id: contact.Id, mail: contact.Email }));

      const payload = {
        mailMessage: {
          subject: `Emergency Notification From`,
          messageBody: notificationMessageBody,
        },
        recipients,
      };

      fetch(`${config.apiBaseUrl}/api/Email/send-mail`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      })
        .then((response) => {
          if (!response.ok) {
            throw new Error('Network response was not ok');
          }
          setIsSuccess(true);
          setTimeout(() => {
            setIsSending(false);
            setIsSuccess(false);
          }, 4500);
        })
        .catch((error) => {
          console.error('There was an error sending the notification!', error);
          setIsSending(false);
        });
    }
  };

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

  const openActionModal = (id) => {
    const contactToEdit = contacts.find(contact => contact.Id === id);
    setEditContact(contactToEdit);
    setIsActionModalOpen(id);
  };

  const closeActionModal = () => {
    setIsActionModalOpen(null);
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
            <div className="table_contacts_wrapper">
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
                      <td className="action-cell">
                        <button className="btn_action" type="button" onClick={() => openActionModal(contact.Id)}>
                          ...
                        </button>

                        {isActionModalOpen === contact.Id && (
                          <div className={`modal action-modal ${isActionModalOpen === contact.Id ? 'action-modal' : ''}`}>
                            <div className="modal_content">
                              <h2>Choose an action</h2>


                              {isActionModalOpen === contact.Id ? (
                                editContact && (
                                  <form onSubmit={handleEditSubmit}>
                                    <input
                                      className="input_edit_modal input_contact"
                                      type="text"
                                      name="Name"
                                      value={editContact.Name || ''}
                                      onChange={handleEditChange}
                                      placeholder="Name"
                                      required
                                    />
                                    <input
                                      className="input_edit_modal input_contact"
                                      type="tel"
                                      name="Phone"
                                      value={editContact.Phone || ''}
                                      onChange={handleEditChange}
                                      placeholder="Phone"
                                      required
                                    />
                                    <input
                                      className="input_edit_modal input_contact"
                                      type="email"
                                      name="Email"
                                      value={editContact.Email || ''}
                                      onChange={handleEditChange}
                                      placeholder="Email"
                                      required
                                    />
                                    <button type="submit" className="btn_create_contact btn_update">Save</button>
                                  </form>
                                )
                              ) : (
                                <div>
                                  <span>{contact.Name}</span>
                                  <span>{contact.Phone}</span>
                                  <span>{contact.Email}</span>
                                  <button onClick={() => openActionModal(contact.Id)}>Edit</button>
                                  <button onClick={() => delAction(contact.Id)}>Delete</button>
                                </div>
                              )}

                              <button className="btn_del_contact" onClick={() => delAction(contact.Id)}>Delete</button>
                              <button className="close_btn" onClick={closeActionModal}>Cancel</button>
                            </div>
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
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
              <textarea
                placeholder="Type message notification:"
                className="textarea_notify_message"
                value={notificationMessageBody}
                onChange={handleMessageBodyChange}
              ></textarea>
            </div>
            <div className="contacts_area_notify">
              <h2>WHO TO NOTIFY?</h2>
              <div className="contacts_selection">
                {contacts.map(contact => (
                  <div key={contact.Id} className="checkbox-contacts">
                    <input
                      type="checkbox"
                      id={`contact-${contact.Id}`}
                      name="notifyContacts"
                      value={contact.Id}
                      onChange={handleContactSelection}
                    />
                    <label htmlFor={`contact-${contact.Id}`}>
                      <span></span>{contact.Name}
                    </label>
                  </div>
                ))}
              </div>
              <button
                type="submit"
                className={`btn_send_notification ${isSending ? 'sending disabled' : ''} ${isSuccess ? 'success disabled' : ''}`}
                disabled={isSending || !isNotifyOpen || isSuccess}
              >
                {isSending ? (isSuccess ? 'SENT' : 'SENDING...') : 'NOTIFY'}
              </button>
            </div>
          </form>
          <button className="notify_btn_close" onClick={handleNotifyButtonClick}>Close</button>
        </div>
      </div>
    </div>
  );
};

export default MainPage;
