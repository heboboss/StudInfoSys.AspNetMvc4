﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StudInfoSys.Models;
using StudInfoSys.Repository;
using StudInfoSys.ViewModels;
using StudInfoSys.Helpers;

namespace StudInfoSys.Controllers
{
    [Authorize]
    public class SubjectGradesRecordController : Controller
    {
        IUnitOfWork _unitOfWork;

        public SubjectGradesRecordController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Indexes the specified registration id.
        /// </summary>
        /// <param name="id">The registration id.</param>
        /// <returns></returns>
        public ActionResult Index(int id)
        {
            return SubjectGradesRecordByRegistrationId(id);
        }

        /// <summary>
        /// Grades record by registration id.
        /// </summary>
        /// <param name="registrationId">The registration id.</param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult SubjectGradesRecordByRegistrationId(int registrationId)
        {
            //var subjectGradesRecords = _unitOfWork.SubjectGradesRecordRepository.SearchFor(sgr => sgr.Registration.Id == id, false).Include(sgr => sgr.Subject);
            //var subjectGradesRecordViewModel = MapListOfSubjectGradesRecordToListOfSubjectGradesRecordViewModel(subjectGradesRecords);
            ViewBag.RegistrationId = registrationId;
            var currentRegistration = _unitOfWork.RegistrationRepository.GetById(registrationId);
            if (currentRegistration == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentFullName = currentRegistration.Student.FullName;
            return View("Index", _unitOfWork.SubjectGradesRecordRepository.SearchFor(sgr => sgr.Registration.Id == registrationId, false)
                .Include(sgr => sgr.Subject)
                .Include(sgr => sgr.Grades)
                );
        }

        public ActionResult Details(int id = 0)
        {
            SubjectGradesRecord subjectgradesrecord = _unitOfWork.SubjectGradesRecordRepository.GetById(id);
            if (subjectgradesrecord == null)
            {
                return HttpNotFound();
            }
            return View(subjectgradesrecord);
        }

        /// <summary>
        /// Creates a SubjectGradesRecord for the specified registration id.
        /// </summary>
        /// <param name="registrationId">The current registration id to which this SubjectGradesRecord is related.</param>
        /// <returns></returns>
        public ActionResult Create(int registrationId)
        {
            var currentRegistration = _unitOfWork.RegistrationRepository.GetById(registrationId);
            if (currentRegistration == null)
            {
                return HttpNotFound();
            }
            var levelIdOfCurrentRegistration = currentRegistration.Degree.LevelId;
            
            var subjectGradesRecordViewModel = new SubjectGradesRecordViewModel
            {
                //Id = 0,
                RegistrationId = registrationId,
                SubjectsList = new SelectList(_unitOfWork.SubjectRepository.GetAll().Where(s => s.LevelId == levelIdOfCurrentRegistration) , "Id", "Name"),
                PeriodsList = _unitOfWork.PeriodRepository.GetAll().Where(p => p.LevelId == levelIdOfCurrentRegistration),
                StudentFullName = currentRegistration.Student.FullName,
                GradeViewModels = new List<GradeViewModel>()
            };
            
            return View(subjectGradesRecordViewModel);
        }

        [HttpPost]
        public ActionResult Create(SubjectGradesRecordViewModel subjectGradesRecordViewModel)
        {
            if (ModelState.IsValid)
            {
                var subjectGradesRecord = MapSubjectGradesRecordViewModel_To_SubjectGradesRecord(subjectGradesRecordViewModel);
                _unitOfWork.SubjectGradesRecordRepository.Insert(subjectGradesRecord);
                _unitOfWork.SubjectGradesRecordRepository.Save();
                return RedirectToAction("Index", new { id = subjectGradesRecordViewModel.RegistrationId });
            }

            //ViewBag.SubjectId = new SelectList(_unitOfWork.SubjectRepository.GetAll().Distinct(), "Id", "SubjectCode", subjectGradesRecordViewModel.SubjectId);
            var levelIdOfCurrentRegistration = _unitOfWork.RegistrationRepository.GetById(subjectGradesRecordViewModel.RegistrationId).Degree.LevelId;
            
            subjectGradesRecordViewModel.SubjectsList = new SelectList(_unitOfWork.SubjectRepository.GetAll().Where(s => s.LevelId == levelIdOfCurrentRegistration), "Id", "Name");
            subjectGradesRecordViewModel.PeriodsList = _unitOfWork.PeriodRepository.GetAll().Where(p => p.LevelId == levelIdOfCurrentRegistration);
            subjectGradesRecordViewModel.GradeViewModels = new List<GradeViewModel>();

            return View(subjectGradesRecordViewModel);
        }

        /// <summary>
        /// Edits the SubjectGradesRecord with the specified id.
        /// </summary>
        /// <param name="id">The SubjectGradesRecord id.</param>
        /// <returns></returns>
        public ActionResult Edit(int id = 0)
        {
            SubjectGradesRecord subjectGradesRecord = _unitOfWork.SubjectGradesRecordRepository.GetById(id);

            if (subjectGradesRecord == null)
            {
                return HttpNotFound();
            }

            int registrationId = subjectGradesRecord.Registration.Id;
            var levelIdOfCurrentRegistration = _unitOfWork.RegistrationRepository.GetById(registrationId).Degree.LevelId;

            var subjectGradesRecordViewModel = MapSubjectGradesRecord_To_SubjectGradesRecordViewModel(subjectGradesRecord);

            subjectGradesRecordViewModel.SubjectsList = new SelectList(_unitOfWork.SubjectRepository.GetAll().Where(s => s.LevelId == levelIdOfCurrentRegistration), "Id", "Name");
            subjectGradesRecordViewModel.PeriodsList = _unitOfWork.PeriodRepository.GetAll().Where(p => p.LevelId == levelIdOfCurrentRegistration);

            return View(subjectGradesRecordViewModel);
        }

        [HttpPost]
        public ActionResult Edit(SubjectGradesRecordViewModel subjectGradesRecordViewModel)
        {
            if (ModelState.IsValid)
            {
                var subjectGradesRecord = MapSubjectGradesRecordViewModel_To_SubjectGradesRecord(subjectGradesRecordViewModel);
                _unitOfWork.SubjectGradesRecordRepository.Update(subjectGradesRecord);
                _unitOfWork.SubjectGradesRecordRepository.Save();
                return RedirectToAction("Index", new { id = subjectGradesRecordViewModel.RegistrationId });
            }
            
            var levelIdOfCurrentRegistration = _unitOfWork.RegistrationRepository.GetById(subjectGradesRecordViewModel.RegistrationId).Degree.LevelId;

            subjectGradesRecordViewModel.SubjectsList = new SelectList(_unitOfWork.SubjectRepository.GetAll().Where(s => s.LevelId == levelIdOfCurrentRegistration), "Id", "Name");
            subjectGradesRecordViewModel.PeriodsList = _unitOfWork.PeriodRepository.GetAll().Where(p => p.LevelId == levelIdOfCurrentRegistration);

            return View(subjectGradesRecordViewModel);
        }

        /// <summary>
        /// Deletes the SubjectGradesRecord with the specified id.
        /// </summary>
        /// <param name="id">The SubjectGradesRecord id.</param>
        /// <returns></returns>
        public ActionResult Delete(int id = 0)
        {
            SubjectGradesRecord subjectgradesrecord = _unitOfWork.SubjectGradesRecordRepository.GetById(id);
            if (subjectgradesrecord == null)
            {
                return HttpNotFound();
            }
            return View(subjectgradesrecord);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            SubjectGradesRecord subjectgradesrecord = _unitOfWork.SubjectGradesRecordRepository.GetById(id);
            foreach (var grade in subjectgradesrecord.Grades.ToList())
            {
                _unitOfWork.GradeRepository.Delete(grade);
            }
            var registrationIdOfCurrentSubjectGradesRecord = subjectgradesrecord.Registration.Id;
            _unitOfWork.SubjectGradesRecordRepository.Delete(subjectgradesrecord);
            _unitOfWork.SubjectGradesRecordRepository.Save();
            return RedirectToAction("Index", new { id = registrationIdOfCurrentSubjectGradesRecord });
        }
        
        private SubjectGradesRecord MapSubjectGradesRecordViewModel_To_SubjectGradesRecord(SubjectGradesRecordViewModel subjectGradesRecordViewModel)
        {
            var subjectGradesRecord = _unitOfWork.SubjectGradesRecordRepository.GetById(subjectGradesRecordViewModel.Id);

            // For editing existing grade record
            if (subjectGradesRecord != null)
            {
                subjectGradesRecord.SubjectId = subjectGradesRecordViewModel.SubjectId;

                foreach (var gradeViewModel in subjectGradesRecordViewModel.GradeViewModels)
                {
                    var grade = _unitOfWork.GradeRepository.GetById(gradeViewModel.Id);
                    grade.GradeValue = gradeViewModel.GradeValue;
                }
            }
            else // For creating new grade record
            {
                subjectGradesRecord =  new SubjectGradesRecord
                {
                    Id = subjectGradesRecordViewModel.Id, 
                    Registration = _unitOfWork.RegistrationRepository.GetById(subjectGradesRecordViewModel.RegistrationId),
                    SubjectId = subjectGradesRecordViewModel.SubjectId,
                    Grades = new List<Grade>()
                };

                foreach (var grade in subjectGradesRecordViewModel.GradeViewModels)
                {
                    subjectGradesRecord.Grades.Add(new Grade
                    {
                        PeriodId = grade.PeriodId,
                        GradeValue = grade.GradeValue
                    });
                }
            }
            
            return subjectGradesRecord;
        }

        private SubjectGradesRecordViewModel MapSubjectGradesRecord_To_SubjectGradesRecordViewModel(SubjectGradesRecord subjectGradesRecord)
        {
            var subjectGradesRecordViewModel = new SubjectGradesRecordViewModel()
            {
                RegistrationId = subjectGradesRecord.Registration.Id,
                SubjectId = subjectGradesRecord.Subject.Id,
                LevelId = subjectGradesRecord.Registration.Degree.LevelId,
                StudentFullName = subjectGradesRecord.Registration.Student.FullName,
                GradeViewModels = new List<GradeViewModel>()
            };

            foreach (var grade in subjectGradesRecord.Grades)
            {
                subjectGradesRecordViewModel.GradeViewModels.Add(new GradeViewModel
                {
                    Id = grade.Id,
                    PeriodId = grade.PeriodId,
                    PeriodName = grade.Period.Name,
                    GradeValue = grade.GradeValue
                });
            }

            return subjectGradesRecordViewModel;
        }

        //private ICollection<Grade> MapListOfGradeViewModelsToListOfGrades(IEnumerable<GradeViewModel> listOfGradeViewModel)
        //{
        //    var listOfGrades = new List<Grade>();
        //    foreach (var grade in listOfGradeViewModel)
        //    {
        //        listOfGrades.Add(MapGradeViewModelToGrade(grade));
        //    }
        //    return listOfGrades;
        //}

        //private Grade MapGradeViewModelToGrade(GradeViewModel gradeViewModel)
        //{
        //    return new Grade
        //        {
        //            PeriodId = gradeViewModel.PeriodId,
        //            GradeValue = gradeViewModel.GradeValue,
        //        };
        //}

        private IEnumerable<SubjectGradesRecordViewModel> MapListOfSubjectGradesRecordToListOfSubjectGradesRecordViewModel(IEnumerable<SubjectGradesRecord> listOfSubjectGradesRecord)
        {
            List<SubjectGradesRecordViewModel> listOfSubjectGradesRecordViewModel = new List<SubjectGradesRecordViewModel>();

            foreach (var subjectGradesRecord in listOfSubjectGradesRecord)
            {
                SubjectGradesRecordViewModel vm = MapSubjectGradesRecord_To_SubjectGradesRecordViewModel(subjectGradesRecord);
                listOfSubjectGradesRecordViewModel.Add(vm);
            }

            return listOfSubjectGradesRecordViewModel;
        }


        protected override void OnException(ExceptionContext filterContext)
        {
            //Log error
            Log.WriteLog(Properties.Settings.Default.LogErrorFile, filterContext.Exception.ToString());
        }


    }
}