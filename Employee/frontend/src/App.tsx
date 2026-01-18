import sirmaLogo from './assets/sirma-logo.png'
import './App.css'
import UploadAndGrid from './components/UploadAndGrid'

function App() {

  return (
    <>
      <div>
        <a href="https://sirma.com/" target="_blank">
          <img src={sirmaLogo} className="logo" alt="Sirma logo" />
        </a>        
      </div>
      <h1>TASK</h1>
      <h2>Pair of employees who have worked together</h2>
      
      <UploadAndGrid />
    </>
  )
}

export default App
