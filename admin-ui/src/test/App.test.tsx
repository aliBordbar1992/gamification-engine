import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import App from '../App'

// Mock the AuthProvider to avoid authentication issues in tests
const MockApp = () => (
  <BrowserRouter>
    <App />
  </BrowserRouter>
)

describe('App', () => {
  it('renders without crashing', () => {
    render(<MockApp />)
    // The app should render without throwing errors
    expect(document.body).toBeDefined()
  })

  it('shows login page when not authenticated', () => {
    render(<MockApp />)
    // Should redirect to login or show login form
    expect(screen.getByText('Gamification Engine')).toBeInTheDocument()
  })
})
