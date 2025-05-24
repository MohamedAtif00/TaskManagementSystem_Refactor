import { useState, useRef, ChangeEvent, DragEvent } from "react";
import { FiUpload } from "react-icons/fi";
import { MdClose, MdPictureAsPdf } from "react-icons/md";
import { BsCheckCircleFill } from "react-icons/bs";

interface FileUploadProps {
  onFileChange: (file: File | null) => void;
}

const FileUpload = ({ onFileChange }: FileUploadProps) => {
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState<string>("");
  const [isDragging, setIsDragging] = useState<boolean>(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragEnter = (e: DragEvent<HTMLDivElement>): void => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e: DragEvent<HTMLDivElement>): void => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const validateFile = (file: File): boolean => {
    const maxSize = 10 * 1024 * 1024; // 10MB
    if (!file.type.includes("pdf")) {
      setError("Please upload PDF files only");
      return false;
    }
    if (file.size > maxSize) {
      setError("File size should not exceed 10MB");
      return false;
    }
    return true;
  };

  const handleDrop = (e: DragEvent<HTMLDivElement>): void => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
    setError("");

    const droppedFile = e.dataTransfer.files[0];
    if (droppedFile && validateFile(droppedFile)) {
      setFile(droppedFile);
      onFileChange(droppedFile);
    }
  };

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>): void => {
    setError("");
    const selectedFile = e.target.files?.[0];
    if (selectedFile && validateFile(selectedFile)) {
      setFile(selectedFile);
      onFileChange(selectedFile);
    }
  };

  const handleReset = (): void => {
    setFile(null);
    setError("");
    onFileChange(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <div className="max-w-xl mx-auto p-6">
      <div
        className={`relative border-2 border-dashed rounded-lg p-8 transition-all ${
          isDragging
            ? "border-blue-500 bg-blue-50"
            : "border-gray-300 hover:border-gray-400"
        } ${error ? "border-red-500 bg-red-50" : ""}`}
        onDragEnter={handleDragEnter}
        onDragOver={handleDragEnter}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        role="button"
        tabIndex={0}
        aria-label="File upload area"
      >
        <input
          type="file"
          ref={fileInputRef}
          className="hidden"
          accept=".pdf"
          onChange={handleFileChange}
          aria-label="File input"
        />

        <div className="text-center">
          {!file ? (
            <>
              <FiUpload className="mx-auto h-12 w-12 text-gray-400" />
              <div className="mt-4">
                <button
                  onClick={(e) =>{e.preventDefault(); fileInputRef.current?.click()}}
                  className="px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
                >
                  Select PDF file
                </button>
                <p className="mt-2 text-sm text-gray-500">
                  or drag and drop your file here
                </p>
              </div>
              <p className="mt-1 text-xs text-gray-500">PDF up to 10MB</p>
            </>
          ) : (
            <div className="flex items-center justify-center space-x-4">
              <MdPictureAsPdf className="h-8 w-8 text-red-500" />
              <span className="text-sm text-gray-500">{file.name}</span>
              <button
                onClick={handleReset}
                className="p-1 rounded-full hover:bg-gray-100"
                aria-label="Remove file"
              >
                <MdClose className="h-5 w-5 text-gray-500" />
              </button>
              <BsCheckCircleFill className="h-5 w-5 text-green-500" />
            </div>
          )}
        </div>

        {error && (
          <div className="absolute bottom-0 left-0 right-0 px-6 py-4 bg-red-100 rounded-b-lg">
            <p className="text-sm text-red-500">{error}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default FileUpload;
